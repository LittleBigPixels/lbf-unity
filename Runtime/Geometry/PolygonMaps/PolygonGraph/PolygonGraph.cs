using System.Collections.Generic;
using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    public partial class PolygonGraph
    {
        public Vector2[] CellCenters;
        public Vector2[] Vertices;
        public int[] HalfEdges;
        public int[] OppositeEdges;
        public int[] CellEdgesStarts;

        public int FirstEdge(int cell) => CellEdgesStarts[cell];
        public int LastEdgeExclusive(int cell) => cell == CellEdgesStarts.Length - 1
            ? HalfEdges.Length
            : CellEdgesStarts[cell + 1];

        public IEnumerable<int> CellEdges(int cell)
        {
            for (int e = FirstEdge(cell); e < LastEdgeExclusive(cell); e++)
                yield return e;
        } 
        
        public IEnumerable<int> CellVertices(int cell)
        {
            for (int e = FirstEdge(cell); e < LastEdgeExclusive(cell); e++)
                yield return HalfEdges[e];
        }

        public IEnumerable<Vector2> CellVertexPositions(int cell)
        {
            for (int e = FirstEdge(cell); e < LastEdgeExclusive(cell); e++)
                yield return Vertices[HalfEdges[e]];
        }
    }
}