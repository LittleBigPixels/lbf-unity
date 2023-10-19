using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace LBF.Geometry.PolygonMaps
{
    public partial class PolygonGraph
    {
        public enum VertexType
        {
            Barycenter,
            Circumenter
        }

        public static PolygonGraph FromDelaunay(TriangleGraph triangleGraph, IBoundaryQuery boundary,
            VertexType vertexType)
        {
            //Construct a list of all voronoi cell vertices
            var voronoiCellVertices = new List<Vector2>(triangleGraph.Edges.Length * 2);
            var circumcenterToCellVertexIndex = new int[triangleGraph.TriangleCount];
            var exteriorEdgeProjectionToCellVertexIndex = new int[triangleGraph.Edges.Length];

            //Compute the circumcenter of all delaunay triangle and if it lies inside or outside the boundary
            Profiler.BeginSample("Compute circumcenters");
            int iCellVertexIndex = 0;
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
                    var position = vertexType == VertexType.Barycenter
                        ? triangleGraph.Barycenter(tri)
                        : triangleGraph.Circumcenter(tri);
                    voronoiCellVertices.Add(position);
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
            var exteriorEdgeProjections = new Vector2[triangleGraph.Edges.Length];
            for (int e = 0; e < triangleGraph.Edges.Length; e++)
            {
                if (!isEdgeExterior[e]) continue;
                var sourceCircumcenter = circumcenters[e / 3];
                var normal = triangleGraph.EdgeNormal(e);
                var boundaryIntersection = boundary.Raycast(new Ray2D(sourceCircumcenter, normal));
                exteriorEdgeProjections[e] = boundaryIntersection.Intersection;
            }

            Profiler.EndSample();

            //Add the projected points to the voronoi vertices
            Profiler.BeginSample("Add projected points to voronoi vertices");
            for (int e = 0; e < triangleGraph.Edges.Length; e++)
            {
                if (!isEdgeExterior[e]) continue;
                exteriorEdgeProjectionToCellVertexIndex[e] = iCellVertexIndex++;
                voronoiCellVertices.Add(exteriorEdgeProjections[e]);
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
                while (true)
                {
                    var tri = currentEdge / 3;
                    var previousEdge = triangleGraph.PreviousEdgeInTriangle(currentEdge);
                    var oppositeEdge = triangleGraph.OppositeHalfEdges[previousEdge];
                    if (isCircumcenterOutsideBoundary[tri] == false)
                    {
                        if (isEdgeExterior[currentEdge])
                        {
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
                        }
                    }

                    if (isEdgeExterior[previousEdge] || oppositeEdge == -1) break;
                    if (oppositeEdge == startEdge) break;

                    currentEdge = oppositeEdge;
                }

                foreach (var index in cellVertexIndices)
                    cellVerticesPosition.Add(voronoiCellVertices[index]);

                //Construct the cell
                cellCenters.Add(Helpers.PolygonCentroid(cellVerticesPosition));
                cellStarts.Add(iEdge);

                for (int i = 0; i < cellEdgeOppositeCell.Count; i++)
                {
                    edges.Add(cellVertexIndices[i]);
                    edgesOppositeCell.Add(cellEdgeOppositeCell[i]);
                    iEdge++;
                }
            }

            Profiler.EndSample();

            //Fix up opposite edges now that everything else is computed and indexed,
            //Using information from the edge cell and the opposite cell
            Profiler.BeginSample("Fix up opposite edges");
            var oppositeEdges = new int[edges.Count];
            //for (int cell = 0; cell < cellStarts.Count; cell++)
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
            graph.Vertices = voronoiCellVertices.ToArray();
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
            //for (int e = 0; e < triangleGraph.Edges.Length; e++)
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
            //for (int e = 0; e < triangleGraph.Edges.Length; e++)
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
            //for (int v = 0; v < triangleGraph.Vertices.Length; v++)
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