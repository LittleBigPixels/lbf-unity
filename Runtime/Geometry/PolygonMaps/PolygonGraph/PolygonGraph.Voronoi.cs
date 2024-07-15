using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace LBF.Geometry.PolygonMaps
{
    public partial class PolygonGraph
    {
        public enum VertexPositionType
        {
            Barycenter,
            Circumcenter
        }

        struct VertexData
        {
            public enum VertexType
            {
                Interior,
                Projection,
                Boundary
            }

            public VertexType Type;
            public Vector2 Position;
            public int BoundarySegmentIndex;

            public VertexData(VertexType type, Vector2 position)
            {
                Type = type;
                Position = position;
                BoundarySegmentIndex = -1;
            }

            public VertexData(Vector2 position, int boundarySegmentIndex)
            {
                Type = VertexType.Projection;
                Position = position;
                BoundarySegmentIndex = boundarySegmentIndex;
            }
        }

        private static bool once = false;

        public static PolygonGraph FromDelaunay(TriangleGraph triangleGraph, IBoundaryQuery boundary,
            VertexPositionType vertexPositionType)
        {
            //Construct a list of all voronoi cell vertices
            var voronoiCellVertices =
                new List<VertexData>(triangleGraph.Edges.Length * 2 + boundary.Vertices.Length);
            var circumcenterToCellVertexIndex = new int[triangleGraph.TriangleCount];
            var exteriorEdgeProjectionToCellVertexIndex = new int[triangleGraph.Edges.Length];
            var boundaryToCellVertexIndex = new int[boundary.Vertices.Length];

            int iCellVertexIndex = 0;

            //Add boundary vertices to the voronoi vertices
            for (int i = 0; i < boundary.Vertices.Length; i++)
            {
                boundaryToCellVertexIndex[i] = iCellVertexIndex;
                voronoiCellVertices.Add(new VertexData(VertexData.VertexType.Boundary, boundary.Vertices[i]));
                iCellVertexIndex++;
            }

            //Compute the circumcenter of all delaunay triangle and if it lies inside or outside the boundary
            Profiler.BeginSample("Compute circumcenters");
            var circumcenters = new Vector2[triangleGraph.TriangleCount];
            var isCircumcenterOutsideBoundary = new bool[triangleGraph.TriangleCount];

            Parallel.For(0, triangleGraph.TriangleCount, (tri) =>
            {
                var c = triangleGraph.Circumcenter(tri);
                circumcenters[tri] = c;
                isCircumcenterOutsideBoundary[tri] = !boundary.Contains(c);
            });
            Profiler.EndSample();

            //Add all circumcenter that lies inside to the voronoi vertices
            Profiler.BeginSample("Create voronoi vertices");
            for (int tri = 0; tri < triangleGraph.TriangleCount; tri++)
            {
                if (!isCircumcenterOutsideBoundary[tri])
                {
                    circumcenterToCellVertexIndex[tri] = iCellVertexIndex++;
                    var position = vertexPositionType == VertexPositionType.Barycenter
                        ? triangleGraph.Barycenter(tri)
                        : triangleGraph.Circumcenter(tri);
                    voronoiCellVertices.Add(new VertexData(VertexData.VertexType.Interior, position));
                }
            }

            Profiler.EndSample();

            //Build a new list of exterior edges after culling triangles whose circumcenters lies outside the boundary
            Profiler.BeginSample("Mark exterior edges");
            var isEdgeExterior = new bool[triangleGraph.Edges.Length];
            for (int e = 0; e < triangleGraph.Edges.Length; e++)
            {
                if (triangleGraph.IsExteriorEdge(e))
                    isEdgeExterior[e] = true;
                else if (isCircumcenterOutsideBoundary[triangleGraph.OppositeHalfEdges[e] / 3])
                    isEdgeExterior[e] = true;
                if (isCircumcenterOutsideBoundary[e / 3])
                    isEdgeExterior[e] = false;
            }

            Profiler.EndSample();

            //Foreach of these exterior edges, project the circumcenter of their triangle to the boundary
            Profiler.BeginSample("Project exterior edges to boundary");
            var exteriorEdgeProjections = new Helpers.BoundaryIntersection[triangleGraph.Edges.Length];
            for (int e = 0; e < triangleGraph.Edges.Length; e++)
            {
                if (!isEdgeExterior[e]) continue;
                var sourceCircumcenter = circumcenters[e / 3];
                var normal = triangleGraph.EdgeNormal(e);
                var boundaryIntersection = boundary.Raycast(new Ray2D(sourceCircumcenter, normal));
                exteriorEdgeProjections[e] = boundaryIntersection;
            }

            Profiler.EndSample();

            //Add the projected points to the voronoi vertices
            Profiler.BeginSample("Add projected points to voronoi vertices");
            for (int e = 0; e < triangleGraph.Edges.Length; e++)
            {
                if (!isEdgeExterior[e]) continue;
                exteriorEdgeProjectionToCellVertexIndex[e] = iCellVertexIndex++;
                var edgeProjection = exteriorEdgeProjections[e];
                voronoiCellVertices.Add(new VertexData(edgeProjection.Intersection, edgeProjection.SegmentIndex));
            }

            Profiler.EndSample();

            //Construct the voronoi cells
            var iEdge = 0;
            var cellCenters = new List<Vector2>(triangleGraph.TriangleCount);
            var cellStarts = new List<int>(triangleGraph.TriangleCount);
            var edges = new List<int>(triangleGraph.Edges.Length * 2);
            var edgesOppositeCell = new List<int>(triangleGraph.Edges.Length * 2);
            Profiler.BeginSample("Create voronoi cells");
            List<int> cellVertexIndices = new List<int>(16);
            List<int> cellEdgeOppositeCell = new List<int>(16);
            List<Vector2> cellVerticesPosition = new List<Vector2>(16);
            for (int v = 0; v < triangleGraph.Vertices.Length; v++)
            {
                cellVertexIndices.Clear();
                cellEdgeOppositeCell.Clear();
                cellVerticesPosition.Clear();

                //Enumerate triangles around the delaunay vertex
                var startEdge = triangleGraph.VerticesToEdge[v];
                var currentEdge = startEdge;

                //Walk backwards to an exterior edge, or to the start
                while (true)
                {
                    var oppositeEdge = triangleGraph.OppositeHalfEdges[currentEdge];
                    if (oppositeEdge == -1) break;

                    var nextEdge = triangleGraph.NextEdgeInTriangle(oppositeEdge);
                    currentEdge = nextEdge;
                    if (isEdgeExterior[nextEdge]) break;
                    if (currentEdge == startEdge) break;
                }

                startEdge = currentEdge;

                var previousTri = -1;
                var firstProjectedVertexIndex = -1;
                while (true)
                {
                    var tri = currentEdge / 3;
                    var previousEdge = triangleGraph.PreviousEdgeInTriangle(currentEdge);
                    var oppositeEdge = triangleGraph.OppositeHalfEdges[previousEdge];
                    if (isCircumcenterOutsideBoundary[tri] == false)
                    {
                        if (isEdgeExterior[currentEdge])
                        {
                            firstProjectedVertexIndex = exteriorEdgeProjectionToCellVertexIndex[currentEdge];
                            cellVertexIndices.Add(exteriorEdgeProjectionToCellVertexIndex[currentEdge]);
                            //The index of the opposite cell is the same as the other vertex in the edge,
                            //which is the start of next edge
                            var nextEdge = triangleGraph.NextEdgeInTriangle(currentEdge);
                            cellEdgeOppositeCell.Add(triangleGraph.Edges[nextEdge]);
                        }

                        cellVertexIndices.Add(circumcenterToCellVertexIndex[tri]);
                        //The index of the opposite cell is the same as the other vertex in the edge,
                        //which is the start of previous edge
                        cellEdgeOppositeCell.Add(triangleGraph.Edges[previousEdge]);

                        if (isEdgeExterior[previousEdge])
                        {
                            cellVertexIndices.Add(exteriorEdgeProjectionToCellVertexIndex[previousEdge]);
                            //The second exterior edge correspond to cell edge with no opposite
                            cellEdgeOppositeCell.Add(-1);

                            //Add boundary edges to complete cell
                            var lastProjectedVertexIndex = exteriorEdgeProjectionToCellVertexIndex[previousEdge];
                            var end = voronoiCellVertices[firstProjectedVertexIndex].BoundarySegmentIndex;
                            var start = voronoiCellVertices[lastProjectedVertexIndex].BoundarySegmentIndex;
                            if (end < start) end += boundary.Vertices.Length;

                            if (once == true) break;
                            
                            for (int i = end; i > start; i--)
                            {
                                cellVertexIndices.Add(boundaryToCellVertexIndex[i % boundaryToCellVertexIndex.Length]);
                                cellEdgeOppositeCell.Add(-1);
                                //once = true;
                            }
                        }
                    }

                    if (isEdgeExterior[previousEdge] || oppositeEdge == -1) break;
                    if (oppositeEdge == startEdge) break;

                    currentEdge = oppositeEdge;
                }

                foreach (var index in cellVertexIndices)
                    cellVerticesPosition.Add(voronoiCellVertices[index].Position);

                //Construct the cell
                cellCenters.Add(triangleGraph.Vertices[v]);
                cellStarts.Add(iEdge);

                for (int i = 0; i < cellEdgeOppositeCell.Count; i++)
                {
                    edges.Add(cellVertexIndices[i]);
                    edgesOppositeCell.Add(cellEdgeOppositeCell[i]);
                    iEdge++;
                }

                if (voronoiCellVertices.First().BoundarySegmentIndex != -1)
                {
                    Debug.Assert(voronoiCellVertices.Last().BoundarySegmentIndex != -1);
                    var start = voronoiCellVertices.First().BoundarySegmentIndex;
                    var end = voronoiCellVertices.Last().BoundarySegmentIndex;
                    if (end < start) end += boundary.Vertices.Length;
                    for (int i = start; i < end; i++)
                    {
                        cellVerticesPosition.Add(boundary.Vertices[i % boundary.Vertices.Length]);
                        edges.Add(cellVertexIndices[i]);
                        edgesOppositeCell.Add(cellEdgeOppositeCell[i]);
                        iEdge++;
                    }
                }
            }

            Profiler.EndSample();

            //Fix up opposite edges now that everything else is computed and indexed,
            //Using information from the edge cell and the opposite cell
            Profiler.BeginSample("Fix up opposite edges");
            var oppositeEdges = new int[edges.Count];
            Parallel.For(0, cellStarts.Count, cell =>
            {
                var edgeStartIndex = cellStarts[cell];
                var edgeEndIndex = cell < cellStarts.Count - 1 ? cellStarts[cell + 1] : edges.Count;
                for (int e = edgeStartIndex; e < edgeEndIndex; e++)
                {
                    oppositeEdges[e] = -1;
                    var nextEdgeIndex = e < edgeEndIndex - 1 ? e + 1 : edgeStartIndex;
                    var edgeEndVertex = edges[nextEdgeIndex];

                    var oppositeCell = edgesOppositeCell[e];
                    if (oppositeCell == -1) continue;

                    var edgeOppositeStartIndex = cellStarts[oppositeCell];
                    var edgeOppositeEndIndex =
                        oppositeCell < cellStarts.Count - 1 ? cellStarts[oppositeCell + 1] : edges.Count;

                    for (int eOpposite = edgeOppositeStartIndex; eOpposite < edgeOppositeEndIndex; eOpposite++)
                        if (edges[eOpposite] == edgeEndVertex)
                            oppositeEdges[e] = eOpposite;
                }
            });

            Profiler.EndSample();

            var graph = new PolygonGraph();
            graph.Vertices = voronoiCellVertices.Select(v => v.Position).ToArray();
            graph.CellCenters = cellCenters.ToArray();
            graph.HalfEdges = edges.ToArray();
            graph.OppositeEdges = oppositeEdges.ToArray();
            graph.CellEdgesStarts = cellStarts.ToArray();

            return graph;
        }

        public static void LoydRelaxation(TriangleGraph triangleGraph, IBoundaryQuery boundary, float w = 1)
        {
            //Compute the circumcenter of all delaunay triangle and if it lies inside or outside the boundary
            Profiler.BeginSample("Compute circumcenters");
            var circumcenters = new Vector2[triangleGraph.TriangleCount];
            var isCircumcenterOutsideBoundary = new bool[triangleGraph.TriangleCount];

            Parallel.For(0, triangleGraph.TriangleCount, (tri) =>
            {
                var c = triangleGraph.Circumcenter(tri);
                circumcenters[tri] = c;
                isCircumcenterOutsideBoundary[tri] = !boundary.Contains(c);
            });
            Profiler.EndSample();

            //Build a new list of exterior edges after culling triangles whose circumcenters lies outside the boundary
            Profiler.BeginSample("Mark exterior edges");
            var isEdgeExterior = new bool[triangleGraph.Edges.Length];
            Parallel.For(0, triangleGraph.Edges.Length, (e) =>
            {
                if (triangleGraph.IsExteriorEdge(e))
                    isEdgeExterior[e] = true;
                else if (isCircumcenterOutsideBoundary[triangleGraph.OppositeHalfEdges[e] / 3])
                    isEdgeExterior[e] = true;
                if (isCircumcenterOutsideBoundary[e / 3])
                    isEdgeExterior[e] = false;
            });
            Profiler.EndSample();

            //Foreach of these exterior edges, project the circumcenter of their triangle to the boundary
            Profiler.BeginSample("Project exterior edges to boundary");
            var exteriorEdgeProjections = new Vector2[triangleGraph.Edges.Length];
            Parallel.For(0, triangleGraph.Edges.Length, (e) =>
            {
                if (!isEdgeExterior[e]) return;
                var sourceCircumcenter = circumcenters[e / 3];
                var normal = triangleGraph.EdgeNormal(e);
                var boundaryIntersection = boundary.Raycast(new Ray2D(sourceCircumcenter, normal));
                exteriorEdgeProjections[e] = boundaryIntersection.Intersection;
            });
            Profiler.EndSample();

            //Construct the voronoi cells
            Profiler.BeginSample("Create voronoi cells");
            Parallel.For(0, triangleGraph.Vertices.Length, (v) =>
            {
                List<Vector2> cellVerticesPosition = new List<Vector2>(16);

                //Enumerate triangles around the delaunay vertex
                var startEdge = triangleGraph.VerticesToEdge[v];
                var currentEdge = startEdge;

                //Walk backwards to an exterior edge, or to the start
                while (true)
                {
                    var oppositeEdge = triangleGraph.OppositeHalfEdges[currentEdge];
                    if (oppositeEdge == -1) break;

                    var nextEdge = triangleGraph.NextEdgeInTriangle(oppositeEdge);
                    currentEdge = nextEdge;
                    if (isEdgeExterior[nextEdge]) break;
                    if (currentEdge == startEdge) break;
                }

                startEdge = currentEdge;

                var previousTri = -1;
                while (true)
                {
                    var tri = currentEdge / 3;
                    var previousEdge = triangleGraph.PreviousEdgeInTriangle(currentEdge);
                    var oppositeEdge = triangleGraph.OppositeHalfEdges[previousEdge];
                    if (isCircumcenterOutsideBoundary[tri] == false)
                    {
                        if (isEdgeExterior[currentEdge])
                        {
                            cellVerticesPosition.Add(exteriorEdgeProjections[currentEdge]);
                            var nextEdge = triangleGraph.NextEdgeInTriangle(currentEdge);
                        }

                        cellVerticesPosition.Add(circumcenters[tri]);

                        if (isEdgeExterior[previousEdge])
                        {
                            cellVerticesPosition.Add(exteriorEdgeProjections[previousEdge]);
                        }
                    }

                    if (isEdgeExterior[previousEdge] || oppositeEdge == -1) break;
                    if (oppositeEdge == startEdge) break;

                    currentEdge = oppositeEdge;
                }

                var centroid = Helpers.PolygonCentroid(cellVerticesPosition);
                triangleGraph.Vertices[v] = triangleGraph.Vertices[v] + (centroid - triangleGraph.Vertices[v]) * w;
            });
            Profiler.EndSample();
        }
    }
}