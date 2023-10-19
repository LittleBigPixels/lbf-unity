using System.Collections.Generic;
using System.Linq;
using LBF.ThirdParties.Delaunator;
using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    public class TriangleGraph
    {
        public int TriangleCount => Edges.Length / 3;

        public readonly Vector2[] Vertices;

        public readonly int[] VerticesToEdge;
        public readonly int[] Edges;
        public readonly int[] OppositeHalfEdges;

        public TriangleGraph(Vector2[] vertices, int[] edges, int[] oppositeHalfEdges)
        {
            Debug.Assert(edges.Length % 3 == 0);
            Debug.Assert(edges.Length == oppositeHalfEdges.Length);

            Vertices = vertices;
            Edges = edges;
            OppositeHalfEdges = oppositeHalfEdges;

            VerticesToEdge = new int[Vertices.Length];
            for (int edge = 0; edge < Edges.Length; edge++)
                VerticesToEdge[Edges[edge]] = edge;
        }

        public TriangleGraph(Vector2[] vertices, int[] edges, int[] oppositeHalfEdges,
            int[] verticesToEdge)
        {
            Debug.Assert(edges.Length % 3 == 0);
            Debug.Assert(edges.Length == oppositeHalfEdges.Length);

            Vertices = vertices;
            Edges = edges;
            OppositeHalfEdges = oppositeHalfEdges;
            VerticesToEdge = verticesToEdge;
        }

        public TriangleGraph(Triangulation delaunay) :
            this(delaunay.coords, delaunay.triangles.ToArray(), delaunay.halfedges.ToArray())
        {
        }

        public int TriangleFirstEdge(int triangle) => triangle * 3;
        public int EdgeTriangle(int edge) => edge / 3;
        public int NextEdgeInTriangle(int edge) => edge % 3 == 2 ? edge - 2 : edge + 1;
        public int PreviousEdgeInTriangle(int edge) => edge % 3 == 0 ? edge + 2 : edge - 1;
        public bool IsExteriorEdge(int edge) => OppositeHalfEdges[edge] == -1;

        public Vector2 HalfEdgeStart(int edge) => Vertices[Edges[edge]];
        public Vector2 HalfEdgeEnd(int edge) => Vertices[Edges[NextEdgeInTriangle(edge)]];

        public Vector2 EdgeNormal(int edge) => (HalfEdgeEnd(edge) - HalfEdgeStart(edge)).OrthogonalLeft().normalized;

        public void BoundedLoydRelaxation(IBoundaryQuery boundary, float w)
        {
            var voronoi = PolygonGraph.FromDelaunay(this, boundary, PolygonGraph.VertexType.Circumenter);
            for (int v = 0; v < Vertices.Length; v++)
            {
                var newPos = voronoi.CellCenters[v];
                Vertices[v] = Vertices[v] + (newPos - Vertices[v]) * w;
            }
        }

        public void EnumerateTrianglesAtVertex(int v, List<int> buffer,
            out int firstExteriorEdge,
            out int lastExteriorEdge)
        {
            var edgeStart = VerticesToEdge[v];
            var edge = edgeStart;
            firstExteriorEdge = -1;
            lastExteriorEdge = -1;

            //Loop edges around the vertex
            while (edge != edgeStart || buffer.Count == 0)
            {
                buffer.Add(edge / 3);
                edge = PreviousEdgeInTriangle(edge);

                if (OppositeHalfEdges[edge] == -1)
                {
                    lastExteriorEdge = edge;
                    break;
                }

                edge = OppositeHalfEdges[edge];
            }

            if (lastExteriorEdge == -1) return;

            buffer.Reverse();
            //Loop backward to the first exterior
            edgeStart = VerticesToEdge[v];
            edge = edgeStart;
            while (true)
            {
                if (IsExteriorEdge(edge))
                {
                    firstExteriorEdge = edge;
                    break;
                }

                edge = OppositeHalfEdges[edge];
                edge = NextEdgeInTriangle(edge);
                buffer.Add(edge / 3);
            }

            buffer.Reverse();
        }

        public Vector2 Barycenter(int tri)
        {
            var v1 = Vertices[Edges[tri * 3]];
            var v2 = Vertices[Edges[tri * 3 + 1]];
            var v3 = Vertices[Edges[tri * 3 + 2]];
            return Helpers.Barycenter(v1, v2, v3);
        }

        public Vector2 Circumcenter(int tri)
        {
            var v1 = Vertices[Edges[tri * 3]];
            var v2 = Vertices[Edges[tri * 3 + 1]];
            var v3 = Vertices[Edges[tri * 3 + 2]];
            return Helpers.Circumcenter(v1, v2, v3);
        }

        public TriangleGraph Copy()
        {
            return new TriangleGraph(Vertices.ToArray(), Edges.ToArray(),
                OppositeHalfEdges.ToArray(), OppositeHalfEdges.ToArray());
        }

        public int LongestEdge(int tri)
        {
            float maxSq = 0;
            int maxIdx = -1;
            for (int i = 0; i < 3; i++)
            {
                var v1 = HalfEdgeStart(tri * 3 + i);
                var v2 = HalfEdgeEnd(tri * 3 + i);
                var lengthSq = Vector2.SqrMagnitude(v2 - v1);
                if (lengthSq > maxSq)
                {
                    maxSq = lengthSq;
                    maxIdx = i;
                }
            }

            return tri * 3 + maxIdx;
        }
    }
}