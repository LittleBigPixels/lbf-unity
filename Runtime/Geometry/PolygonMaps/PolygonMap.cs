using System.Collections.Generic;
using System.Linq;
using LBF.ThirdParties.Delaunator;
using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    public class Cell
    {
        public int Index;
        public Vector2 Center;
        public Cell[] Neighbours;
        public int[] Edges;
    }

    public class PolygonMap
    {
        public int PolygonCount => m_polygons.Length;
        public Cell[] Polygons => m_polygons;
        public PolygonGraph Graph => m_graph;
        public IBoundaryQuery Boundary => m_boundary;

        PolygonGraph m_graph;
        Cell[] m_polygons;
        IBoundaryQuery m_boundary;

        public PolygonMap()
        {
        }

        public static PolygonMap FromPoints(List<Vector2> delaunayPoints, IBoundaryQuery boundary)
        {
            var delaunay = new Triangulation(delaunayPoints.ToArray());
            var triangleGraph = new TriangleGraph(delaunay);
            return new PolygonMap(triangleGraph, boundary);
        }

        public PolygonMap(TriangleGraph delaunayTriangulation, IBoundaryQuery boundary)
        {
            m_boundary = boundary;
            m_graph = PolygonGraph.FromDelaunay(delaunayTriangulation, boundary,
                PolygonGraph.VertexType.Barycenter);
            m_polygons = new Cell[m_graph.CellCenters.Length];

            for (int cell = 0; cell < m_graph.CellCenters.Length; cell++)
                m_polygons[cell] = new Cell();

            var edgeToCell = new int[m_graph.HalfEdges.Length];
            for (int cell = 0; cell < m_graph.CellCenters.Length; cell++)
                foreach (var e in m_graph.CellEdges(cell))
                    edgeToCell[e] = cell;

            for (int cell = 0; cell < m_graph.CellCenters.Length; cell++)
            {
                var firstEdge = m_graph.FirstEdge(cell);
                var lastEdge = m_graph.LastEdgeExclusive(cell);

                m_polygons[cell].Index = cell;
                m_polygons[cell].Center = m_graph.CellCenters[cell];
                m_polygons[cell].Edges = Enumerable.Range(firstEdge, lastEdge - firstEdge).ToArray();
                m_polygons[cell].Neighbours = Enumerable.Range(firstEdge, lastEdge - firstEdge).ToArray()
                    .Select(e => m_graph.OppositeEdges[e])
                    .Where(e => e != -1)
                    .Select(e => edgeToCell[e])
                    .Select(cell => m_polygons[cell])
                    .ToArray();
            }
        }
    }
}