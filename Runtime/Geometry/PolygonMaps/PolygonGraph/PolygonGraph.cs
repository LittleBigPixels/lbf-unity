using System.Collections.Generic;
using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    public partial class PolygonGraph
    {
        //Cell interior center positions
        //Sorted by cell indices
        public Vector2[] CellCenters;
        
        //Cell border vertex positions
        //In no particular order
        public Vector2[] Vertices;
        
        //Cell border edges, as indices into Vertices
        //Edge are ordered by cell, in clockwise orders
        public int[] HalfEdges;
        
        //Cell border edges, as indices into HalfEdges
        //In no particular order
        //OppositeEdges corresponds to the edge in neighbouring cell
        public int[] OppositeEdges;
        
        //Index of the first edge in a cell, as indices into HalfEdges
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