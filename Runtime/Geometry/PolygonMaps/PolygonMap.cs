using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBF.Structures;
using LBF.ThirdParties.Delaunator;
using log4net.Util;
using UnityEditor.UnityLinker;
using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    public class Cell
    {
        public int Index;
        public Vector2 Center;
        public Cell[] Neighbours;
        public Edge[] Edges;
        public Vertex[] Vertices;
    }

    public class Edge
    {
        public Vertex StartVertex => Vertices[0];
        public Vertex EndVertex => Vertices[1];

        public Vector2 Start => Vertices[0].Position;
        public Vector2 End => Vertices[1].Position;

        public int Index;
        public Vertex[] Vertices;
        public Cell[] AdjacentCells;
    }

    public class Vertex
    {
        public int Index;
        public Vector2 Position;
        public Edge[] Edges;
        public Vertex[] Neighbours;
        public Cell[] AdjacentCells;
    }
    public partial class PolygonMap
    {
        public int PolygonCount => m_cells.Length;
        public int VertexCount => m_vertices.Length;

        public Cell[] Cells => m_cells;
        public Edge[] Edges => m_edges;
        public Vertex[] Vertices => m_vertices;

        public PolygonGraph Graph => m_graph;
        public IBoundaryQuery Boundary => m_boundary;

        PolygonGraph m_graph;
        Cell[] m_cells;
        Edge[] m_edges;
        Vertex[] m_vertices;

        IBoundaryQuery m_boundary;

        public PolygonMap() { }

        public static PolygonMap FromPoints(Vector2[] delaunayPoints, IBoundaryQuery boundary)
        {
            var triangleGraph = TriangleGraph.CreateDelaunay(delaunayPoints);
            return new PolygonMap(triangleGraph, boundary);
        }

        public PolygonMap(TriangleGraph delaunayTriangulation, IBoundaryQuery boundary)
        {
            m_boundary = boundary;
            m_graph = PolygonGraph.FromDelaunay(delaunayTriangulation, boundary, PolygonGraph.VertexPositionType.Barycenter);

            //Initialise cells and vertices arrays
            m_cells = new Cell[m_graph.CellCenters.Length];
            for (int cell = 0; cell < m_graph.CellCenters.Length; cell++)
                m_cells[cell] = new Cell();
            m_vertices = new Vertex[m_graph.Vertices.Length];
            for (int vertex = 0; vertex < m_graph.Vertices.Length; vertex++)
                m_vertices[vertex] = new Vertex();

            //Map graph half-edges to cells and map edges
            var halfEdgeToCell = new Dictionary<int, Cell>();
            for (int iCell = 0; iCell < m_graph.CellCenters.Length; iCell++)
            {
                var firstEdge = m_graph.FirstEdge(iCell);
                var lastEdge = m_graph.LastEdgeExclusive(iCell);
                for (int i = 0; i < lastEdge - firstEdge; i++)
                {
                    halfEdgeToCell[firstEdge + i] = m_cells[iCell];
                }
            }

            //Initialise cells and edges
            var halfEdgeToMapEdge = new Dictionary<int, Edge>();
            var vertexToEdges = new MultiDictionary<int, Edge>();
            var vertexToNeighbours = new MultiDictionary<int, Vertex>();
            var vertexToCells = new MultiDictionary<int, Cell>();
            for (int iCell = 0; iCell < m_graph.CellCenters.Length; iCell++)
            {
                var cell = m_cells[iCell];
                var center = m_graph.CellCenters[iCell];

                var firstEdge = m_graph.FirstEdge(iCell);
                var lastEdge = m_graph.LastEdgeExclusive(iCell);

                cell.Index = iCell;
                cell.Center = center;
                cell.Vertices = Enumerable.Range(firstEdge, lastEdge - firstEdge).Select(i => m_vertices[m_graph.HalfEdges[i]]).ToArray();
                cell.Edges = new Edge[lastEdge - firstEdge];

                var neighbours = new List<Cell>();
                for (int i = 0; i < lastEdge - firstEdge; i++)
                {
                    var edge = firstEdge + i;
                    var startVertex = m_graph.HalfEdges[firstEdge + i];
                    var endVertex = m_graph.HalfEdges[firstEdge + (i + 1) % cell.Vertices.Count()];

                    var oppositeEdge = m_graph.OppositeEdges[firstEdge + i];
                    var oppositeCell = oppositeEdge != -1 ? halfEdgeToCell[oppositeEdge] : null;
                    if (oppositeCell != null)
                        neighbours.Add(halfEdgeToCell[oppositeEdge]);

                    if (oppositeCell == null)
                    {
                        cell.Edges[i] = new Edge()
                        {
                            Index = halfEdgeToMapEdge.Values.Count,
                            Vertices = new Vertex[] { m_vertices[startVertex], m_vertices[endVertex] },
                            AdjacentCells = new Cell[] { cell },
                        };
                        vertexToEdges.Add(startVertex, cell.Edges[i]);
                        vertexToEdges.Add(endVertex, cell.Edges[i]);
                        vertexToNeighbours.Add(startVertex, m_vertices[endVertex]);
                        vertexToNeighbours.Add(endVertex, m_vertices[startVertex]);
                        vertexToCells.Add(startVertex, cell);
                        vertexToCells.Add(endVertex, cell);
                        
                        halfEdgeToMapEdge[edge] = cell.Edges[i];
                    }
                    else if (halfEdgeToMapEdge.ContainsKey(oppositeEdge))
                    {
                        cell.Edges[i] = halfEdgeToMapEdge[oppositeEdge];
                    }
                    else
                    {
                        cell.Edges[i] = new Edge()
                        {
                            Index = halfEdgeToMapEdge.Values.Count,
                            Vertices = new Vertex[] { m_vertices[startVertex], m_vertices[endVertex] },
                            AdjacentCells = new Cell[] { cell, oppositeCell },
                        };
                        vertexToEdges.Add(startVertex, cell.Edges[i]);
                        vertexToEdges.Add(endVertex, cell.Edges[i]);
                        vertexToNeighbours.Add(startVertex, m_vertices[endVertex]);
                        vertexToNeighbours.Add(endVertex, m_vertices[startVertex]);
                        vertexToCells.Add(startVertex, cell);
                        vertexToCells.Add(endVertex, cell);
                        vertexToCells.Add(startVertex, oppositeCell);
                        vertexToCells.Add(endVertex, oppositeCell);
                        
                        halfEdgeToMapEdge[edge] = cell.Edges[i];
                    }
                }

                cell.Neighbours = neighbours.ToArray();
            }

            m_edges = halfEdgeToMapEdge.Values.ToArray();

            for (int vertex = 0; vertex < m_graph.Vertices.Length; vertex++)
            {
                m_vertices[vertex].Index = vertex;
                m_vertices[vertex].Position = m_graph.Vertices[vertex];
                m_vertices[vertex].Neighbours = vertexToNeighbours[vertex].ToArray();
                m_vertices[vertex].Edges = vertexToEdges[vertex].ToArray();
                m_vertices[vertex].AdjacentCells = vertexToCells[vertex].Distinct().ToArray();
            }
        }
    }
}