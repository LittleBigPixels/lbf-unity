using System.Collections.Generic;
using System.Linq;
using LBF.Structures;
using UnityEngine;

namespace LBF.Math.Algorithms {
    public class MarchingSquarePerimeter
    {
        public class VertexData
        {
            public Vector2 Position;
            public EdgeData EdgeA;
            public EdgeData EdgeB;
        }

        public class EdgeData
        {
            public Point VertexA;
            public Point VertexB;
        }

        SpatialGrid<VertexData> m_verticesGrid;
        public SpatialGrid<VertexData> Vertices {
            get { return m_verticesGrid; }
        }

        List<EdgeData> m_edges;
        public List<EdgeData> Edges {
            get { return m_edges; }
        }

        List<VertexData> m_contour;
        public List<VertexData> Contour {
            get { return m_contour; }
        }

        public MarchingSquarePerimeter(SpatialGrid<bool> data)
        {
            //Create a grid where each cell is centered on the vertices
            m_verticesGrid = new SpatialGrid<VertexData>(
                data.Min - data.CellSize * 0.5f, data.Max + data.CellSize * 0.5f, data.CountX + 1, data.CountY + 1);

            foreach (var index in m_verticesGrid.Indices)
                m_verticesGrid[index] = new VertexData() { Position = m_verticesGrid.GetCellCenter(index) + 0 * data.CellSize * 1.0f };

            m_edges = new List<EdgeData>();

            foreach (var index in data.Indices)
            {
                if (data[index] == false) continue;

                var top = new Point(index.x, index.y + 1);
                if (data.Contains(top) == false || data[top] == false)
                    m_edges.Add(new EdgeData() { VertexA = new Point(index.x + 0, index.y + 1), VertexB = new Point(index.x + 1, index.y + 1) });

                var bottom = new Point(index.x, index.y - 1);
                if (data.Contains(bottom) == false || data[bottom] == false)
                    m_edges.Add(new EdgeData() { VertexA = new Point(index.x + 1, index.y + 0), VertexB = new Point(index.x + 0, index.y + 0) });

                var left = new Point(index.x - 1, index.y);
                if (data.Contains(left) == false || data[left] == false)
                    m_edges.Add(new EdgeData() { VertexA = new Point(index.x + 0, index.y + 0), VertexB = new Point(index.x + 0, index.y + 1) });

                var right = new Point(index.x + 1, index.y);
                if (data.Contains(right) == false || data[right] == false)
                    m_edges.Add(new EdgeData() { VertexA = new Point(index.x + 1, index.y + 1), VertexB = new Point(index.x + 1, index.y + 0) });
            }

            foreach (var edge in m_edges)
            {
                m_verticesGrid[edge.VertexA].EdgeB = edge;
                m_verticesGrid[edge.VertexB].EdgeA = edge;
            }

            m_contour = new List<VertexData>();
            if (m_edges.Count == 0) return;

            var currentEdge = m_edges.First();
            for (int i = 0; i < m_edges.Count; i++)
            {
                VertexData currentVertex = m_verticesGrid[currentEdge.VertexB];
                m_contour.Add(currentVertex);

                currentEdge = currentVertex.EdgeB;
            }
        }
    }
}