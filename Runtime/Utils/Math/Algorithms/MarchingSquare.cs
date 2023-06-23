using System;
using System.Collections.Generic;
using LBF.Structures;
using UnityEngine;

namespace LBF.Math.Algorithms {
    public class MarchingSquare
    {
        //Grid edge indices ares as follow:
        //      1
        //     0 2
        //      3

        //Grid corner indices ares as follow:
        //     0 1
        //     3 2

        class GridCell
        {
            public int[] Edges;
        }

        class GridEdge
        {
            public Vector2 A;
            public Vector2 B;
            public Vector2 HalfPoint;

            public Point GridCellA;
            public Point GridCellB;
        }

        class ContourEdge
        {
            public int EdgeA;
            public int EdgeB;
        }

        SpatialGrid<float> m_inputGrid;
        SpatialGrid<GridCell> m_cellGrid;
        List<GridEdge> m_edges;
        List<Vector2> m_edgesInterpolatedPoints;
        List<ContourEdge> m_contourEdges;

        ContourEdge[][] m_edgeTable = new ContourEdge[16][]
        {
            null,   //0000
      
            new ContourEdge[] { new ContourEdge() { EdgeA = 0, EdgeB = 1 } },      //0001
            new ContourEdge[] { new ContourEdge() { EdgeA = 1, EdgeB = 2 } },      //0010
            new ContourEdge[] { new ContourEdge() { EdgeA = 0, EdgeB = 2 } },      //0011
            new ContourEdge[] { new ContourEdge() { EdgeA = 2, EdgeB = 3 } },      //0100
            new ContourEdge[] { new ContourEdge() { EdgeA = 3, EdgeB = 0 } ,    new ContourEdge() { EdgeA = 2, EdgeB = 1 } },  //0101
            new ContourEdge[] { new ContourEdge() { EdgeA = 1, EdgeB = 3 } },      //0110
            new ContourEdge[] { new ContourEdge() { EdgeA = 0, EdgeB = 3 } },      //0111
            new ContourEdge[] { new ContourEdge() { EdgeA = 3, EdgeB = 0 } },      //1000   
            new ContourEdge[] { new ContourEdge() { EdgeA = 1, EdgeB = 3 } },      //1001
            new ContourEdge[] { new ContourEdge() { EdgeA = 3, EdgeB = 2 } ,    new ContourEdge() { EdgeA = 1, EdgeB = 0 } },  //1010
            new ContourEdge[] { new ContourEdge() { EdgeA = 3, EdgeB = 2 } },      //1011
            new ContourEdge[] { new ContourEdge() { EdgeA = 0, EdgeB = 2 } },      //1100  
            new ContourEdge[] { new ContourEdge() { EdgeA = 2, EdgeB = 1 } },      //1101
            new ContourEdge[] { new ContourEdge() { EdgeA = 1, EdgeB = 0 } },      //1110  
        
            null,   //1111
        };

        public MarchingSquare(SpatialGrid<float> discreteField, Func<Vector2, float> sampler, float threshold)
        {
            m_inputGrid = discreteField;
            m_cellGrid = new SpatialGrid<GridCell>(m_inputGrid.Min, m_inputGrid.Max, m_inputGrid.CountX, m_inputGrid.CountY);
            m_edges = new List<GridEdge>();
            m_contourEdges = new List<ContourEdge>();

            m_edgesInterpolatedPoints = new List<Vector2>(m_edges.Count);

            foreach (var index in m_cellGrid.Indices)
            {
                m_cellGrid[index] = new GridCell();
                m_cellGrid[index].Edges = new int[4] { -1, -1, -1, -1 };
            }

            for (int i = 0; i < m_cellGrid.CountX; i++)
            {
                for (int j = 0; j < m_cellGrid.CountY; j++)
                {
                    if (i > 0)
                    {
                        var cellCenter = m_cellGrid.GetCellCenter(i, j);
                        var leftEdge = new GridEdge();
                        leftEdge.A = new Vector2(cellCenter.x - m_cellGrid.CellSize.x * 0.5f, cellCenter.y + m_cellGrid.CellSize.y * 0.5f);
                        leftEdge.B = new Vector2(cellCenter.x - m_cellGrid.CellSize.x * 0.5f, cellCenter.y - m_cellGrid.CellSize.y * 0.5f);
                        leftEdge.HalfPoint = new Vector2(cellCenter.x - m_cellGrid.CellSize.x * 0.5f, cellCenter.y);

                        leftEdge.GridCellA = new Point(i - 1, j);
                        leftEdge.GridCellB = new Point(i, j);

                        var idx = m_edges.Count;
                        m_edges.Add(leftEdge);

                        m_cellGrid[leftEdge.GridCellA].Edges[2] = idx;
                        m_cellGrid[leftEdge.GridCellB].Edges[0] = idx;
                    }

                    if (j > 0)
                    {
                        var cellCenter = m_cellGrid.GetCellCenter(i, j);
                        var bottom = new GridEdge();
                        bottom.A = new Vector2(cellCenter.x + m_cellGrid.CellSize.x * 0.5f, cellCenter.y - m_cellGrid.CellSize.y * 0.5f);
                        bottom.B = new Vector2(cellCenter.x - m_cellGrid.CellSize.x * 0.5f, cellCenter.y - m_cellGrid.CellSize.y * 0.5f);
                        bottom.HalfPoint = new Vector2(cellCenter.x, cellCenter.y - m_cellGrid.CellSize.y * 0.5f);

                        bottom.GridCellA = new Point(i, j - 1);
                        bottom.GridCellB = new Point(i, j);

                        var idx = m_edges.Count;
                        m_edges.Add(bottom);

                        m_cellGrid[bottom.GridCellA].Edges[1] = idx;
                        m_cellGrid[bottom.GridCellB].Edges[3] = idx;
                    }
                }
            }

            foreach(var edge in m_edges)
            {
                var valA = sampler(edge.A);
                var valB = sampler(edge.B);
                float k = Mathf.InverseLerp(valA, valB,threshold);
                m_edgesInterpolatedPoints.Add(k * edge.B + (1 - k) * edge.A);
            }

            foreach (var index in m_cellGrid.Indices)
            {
                var cell = m_cellGrid[index];
                var cellCenter = m_cellGrid.GetCellCenter(index);
                var cellSize = m_cellGrid.CellSize;
                int mask = 0;

                var cornerOffset0 = new Vector2(-cellSize.x * 0.5f, +cellSize.y * 0.5f);
                if (sampler(cellCenter + cornerOffset0) > threshold)
                    mask |= 1 << 0;

                var cornerOffset1 = new Vector2(+cellSize.x * 0.5f, +cellSize.y * 0.5f);
                if (sampler(cellCenter + cornerOffset1) > threshold)
                    mask |= 1 << 1;

                var cornerOffset2 = new Vector2(+cellSize.x * 0.5f, -cellSize.y * 0.5f);
                if (sampler(cellCenter + cornerOffset2) > threshold)
                    mask |= 1 << 2;

                var cornerOffset3 = new Vector2(-cellSize.x * 0.5f, -cellSize.y * 0.5f);
                if (sampler(cellCenter + cornerOffset3) > threshold)
                    mask |= 1 << 3;

                var cornerTableEntry = m_edgeTable[mask];
                if (cornerTableEntry != null)
                    foreach (var cornerEdge in cornerTableEntry)
                        m_contourEdges.Add(new ContourEdge() { EdgeA = cell.Edges[cornerEdge.EdgeA], EdgeB = cell.Edges[cornerEdge.EdgeB] });
            }
        }
    }
}