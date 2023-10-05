using System;
using System.Collections.Generic;
using LBF.Helpers;
using UnityEngine;
using LBF.Math.Geometry.Polygon;

namespace LBF.Math.Algorithms
{
    public class ConvexDecomposition
    {
        public class SubPolygon
        {
            public Polygon MainPolygon;
            public List<int> Indices;

            public SubPolygon(Polygon p, List<int> indices)
            {
                MainPolygon = p;
                Indices = indices;
            }

            public int GetLoopingIndex(int i)
            {
                return (i + Indices.Count) % Indices.Count;
            }

            public Vector2 GetVertex(int i)
            {
                return MainPolygon.Points[Indices[(i + Indices.Count) % Indices.Count]];
            }

            public Tuple<Vector2, Vector2> GetEdgeTo(int i)
            {
                return new Tuple<Vector2, Vector2>(GetVertex(i - 1), GetVertex(i));
            }

            public bool IsClockWise()
            {
                float sum = 0;
                for (int i = 0; i < Indices.Count; i++)
                {
                    var edge = GetEdgeTo(i);
                    sum += (edge.Item2.x - edge.Item1.x) * (edge.Item2.y + edge.Item1.y);
                }

                return sum >= 0;
            }
        }

        public static List<SubPolygon> ConvexPolygons(Polygon p)
        {
            var indices = new List<int>(p.Points.Length);
            for (int i = 0; i < p.Points.Length; i++)
                indices.Add(i);
            SubPolygon startPolygon = new SubPolygon(p, indices);

            List<SubPolygon> convexSubPolygons = new List<SubPolygon>();
            Queue<SubPolygon> subPolygonsToTest = new Queue<SubPolygon>();

            subPolygonsToTest.Enqueue(startPolygon);

            int loopMax = 500;
            while (subPolygonsToTest.Count > 0 && loopMax-- > 0)
            {
                var subPolygon = subPolygonsToTest.Dequeue();

                if (subPolygon.Indices.Count == 3)
                {
                    convexSubPolygons.Add(subPolygon);
                    continue;
                }

                List<int> reflexPointsIndexInSubPolygon = FindReflexPoints(subPolygon);
                if (reflexPointsIndexInSubPolygon.Count == 0)
                {
                    convexSubPolygons.Add(subPolygon);
                    continue;
                }

                var reflexPoint = reflexPointsIndexInSubPolygon[0];

                var bestValidVertex = -1;
                var firstEdge = subPolygon.GetEdgeTo(reflexPoint + 1);
                for (int iStep = 2; iStep < subPolygon.Indices.Count - 1; iStep++)
                {
                    var targetVertex = subPolygon.GetLoopingIndex(reflexPoint + iStep);

                    //Check if intersect with subpolygon
                    bool intersects = false;
                    for (int iEdge = 0; iEdge < subPolygon.Indices.Count; iEdge++)
                    {
                        var reflexVertexPosition = subPolygon.GetVertex(reflexPoint);
                        var targetVertexPosition = subPolygon.GetVertex(targetVertex);
                        var edge = subPolygon.GetEdgeTo(iEdge);
                        if (edge.Item1 == reflexVertexPosition || edge.Item2 == reflexVertexPosition)
                            continue;
                        if (edge.Item1 == targetVertexPosition || edge.Item2 == targetVertexPosition)
                            continue;

                        if (PolygonHelper.IntersectSegments(reflexVertexPosition, targetVertexPosition, edge.Item1, edge.Item2))
                            intersects = true;
                    }

                    //If this vertex is invalid and we have a valid vertex stop here
                    if (intersects && bestValidVertex != -1)
                        break;

                    //Check if we're not creating a new concave point 
                    bool concaveAngle = false;
                    var lastEdgeDir = subPolygon.GetVertex(reflexPoint) - subPolygon.GetVertex(targetVertex);
                    var isCw = subPolygon.IsClockWise();
                    var cwSign = isCw ? 1 : -1;
                    var sign = VectorExtensions.CrossDet(lastEdgeDir, firstEdge.Item2 - firstEdge.Item1);

                    if (sign * cwSign > 0)
                        concaveAngle = true;

                    //If this vertex is invalid and we have a valid vertex stop here
                    if (concaveAngle && bestValidVertex != -1)
                        break;

                    if (!intersects && !concaveAngle)
                        bestValidVertex = targetVertex;
                }

                Debug.Assert(bestValidVertex != -1);

                //Cut the polygon at vertices 'firstValidVertex' and 'reflexPointIndex'
                var minIndex = Mathf.Min(bestValidVertex, reflexPoint);
                var maxIndex = Mathf.Max(bestValidVertex, reflexPoint);
                var firstSubPolygonIndices = new List<int>();
                var secondSubPolygonIndices = new List<int>();
                for (int i = 0; i < subPolygon.Indices.Count; i++)
                {
                    var index = subPolygon.Indices[i];
                    if (i <= minIndex || i >= maxIndex) firstSubPolygonIndices.Add(index);
                    if (i >= minIndex && i <= maxIndex) secondSubPolygonIndices.Add(index);
                }
                subPolygonsToTest.Enqueue(new SubPolygon(p, firstSubPolygonIndices));
                subPolygonsToTest.Enqueue(new SubPolygon(p, secondSubPolygonIndices));
            }

            return convexSubPolygons;
        }

        private static List<int> FindReflexPoints(SubPolygon subPolygon)
        {
            var isCw = subPolygon.IsClockWise();
            var cwSign = isCw ? 1 : -1;

            //Find reflex points in the sub polygon
            List<int> reflexPoints = new List<int>();
            for (int i = 0; i < subPolygon.Indices.Count; i++)
            {
                var edge1 = subPolygon.GetEdgeTo(i);
                var edge2 = subPolygon.GetEdgeTo(i + 1);
                var sign = VectorExtensions.CrossDet(edge1.Item2 - edge1.Item1, edge2.Item2 - edge2.Item1);
                if (sign * cwSign > 0)
                    reflexPoints.Add(i);
            }

            return reflexPoints;
        }
    }
}
