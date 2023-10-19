using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace LBF.Geometry.PolygonMaps
{
    public static partial class Helpers
    {
        public static Vector2 CentroidAroundInteriorVertex(TriangleGraph triangleGraph,
            IBoundaryQuery boundary, List<int> triangles, List<Vector2> cellVerticesBuffer)
        {
            Profiler.BeginSample("Compute Circumcenters");
            cellVerticesBuffer.Clear();
            Vector2[] circumcenters = new Vector2[triangles.Count];
            for (int i = 0; i < triangles.Count; i++)
            {
                circumcenters[i] = triangleGraph.Circumcenter(triangles[i]);
                if (float.IsInfinity(circumcenters[i].x))
                    circumcenters[i] = triangleGraph.Barycenter(triangles[i]);
            }

            Profiler.EndSample();

            Profiler.BeginSample("Build cell vertices");
            for (int i = 0; i < triangles.Count; i++)
            {
                var c1 = circumcenters[i];
                var c2 = circumcenters[(i + 1) % triangles.Count];

                Profiler.BeginSample("Check boundary");
                var isC1In = boundary.Contains(c1);
                var isC2In = boundary.Contains(c2);
                Profiler.EndSample();

                //Circumcenters can be outside of their triangle, and outside of the boundary
                //Clip accordingly
                if (isC1In && isC2In)
                {
                    cellVerticesBuffer.Add(c1);
                }
                else if (isC1In && !isC2In && !float.IsInfinity(c2.x))
                {
                    Profiler.BeginSample("RaycastBoundary");
                    var boundaryIntersection = boundary.Raycast(new Ray2D(c1, c2 - c1));
                    Profiler.EndSample();
                    cellVerticesBuffer.Add(c1);
                    cellVerticesBuffer.Add(boundaryIntersection.Intersection);
                }
                else if (!isC1In && isC2In && !float.IsInfinity(c1.x))
                {
                    Profiler.BeginSample("RaycastBoundary");
                    var boundaryIntersection = boundary.Raycast(new Ray2D(c2, c1 - c2));
                    Profiler.EndSample();
                    cellVerticesBuffer.Add(boundaryIntersection.Intersection);
                }
                else if (!isC1In && !isC2In)
                {
                }
            }

            Profiler.EndSample();

            Profiler.BeginSample("PolygonCentroid");
            var centroid = Helpers.PolygonCentroid(cellVerticesBuffer);
            Profiler.EndSample();

            return centroid;
        }

        public static Vector2 CentroidAroundExteriorPoint(TriangleGraph triangleGraph,
            IBoundaryQuery boundary,
            List<int> triangleIndexBuffer, int firstExteriorEdge,
            int lastExteriorEdge,
            List<Vector2> cellVerticesBuffer)
        {
            List<Vector2> circumcenters = new List<Vector2>();
            for (int i = 0; i < triangleIndexBuffer.Count; i++)
            {
                var tri = triangleIndexBuffer[i];
                var c = triangleGraph.Circumcenter(tri);
                if (!float.IsInfinity(c.x))
                    circumcenters.Add(c);
            }

            if (boundary.Contains(circumcenters[0]))
            {
                var edgeNormal = triangleGraph.EdgeNormal(firstExteriorEdge);
                var boundaryCrossingRay = new Ray2D(circumcenters[0], edgeNormal);
                var boundaryCrossingPoint = boundary.Raycast(boundaryCrossingRay);
                circumcenters.Insert(0, boundaryCrossingPoint.Intersection);
            }

            if (boundary.Contains(circumcenters[^1]))
            {
                var edgeNormal = triangleGraph.EdgeNormal(lastExteriorEdge);
                var boundaryCrossingRay = new Ray2D(circumcenters[^1], edgeNormal);
                var boundaryCrossingPoint = boundary.Raycast(boundaryCrossingRay);
                circumcenters.Add(boundaryCrossingPoint.Intersection);
            }

            for (int i = 0; i < circumcenters.Count; i++)
            {
                var c1 = circumcenters[i];
                var c2 = circumcenters[(i + 1) % circumcenters.Count];

                var isC1In = boundary.Contains(c1);
                var isC2In = boundary.Contains(c2);

                if (isC1In && isC2In)
                {
                    cellVerticesBuffer.Add(c1);
                }
                else if (isC1In && !isC2In)
                {
                    cellVerticesBuffer.Add(c1);
                    var boundaryIntersection = boundary.Raycast(new Ray2D(c1, c2 - c1));
                    if (boundaryIntersection.SegmentIndex != -1)
                        cellVerticesBuffer.Add(boundaryIntersection.Intersection);
                }
                else if (!isC1In && isC2In)
                {
                    var boundaryIntersection = boundary.Raycast(new Ray2D(c2, c1 - c2));
                    if (boundaryIntersection.SegmentIndex != -1)
                        cellVerticesBuffer.Add(boundaryIntersection.Intersection);
                }
                else if (!isC1In && !isC2In)
                {
                    //Nothing
                }
            }

            return Helpers.PolygonCentroid(cellVerticesBuffer);
        }

        public static Vector2 PolygonCentroid(IList<Vector2> vertices)
        {
            Vector2 centroid = Vector2.zero;
            float signedArea = 0f;
            float x0, y0, x1, y1, a;

            for (int i = 0; i < vertices.Count; i++)
            {
                x0 = vertices[i].x;
                y0 = vertices[i].y;
                x1 = vertices[(i + 1) % vertices.Count].x;
                y1 = vertices[(i + 1) % vertices.Count].y;
                a = x0 * y1 - x1 * y0;
                signedArea += a;
                centroid.x += (x0 + x1) * a;
                centroid.y += (y0 + y1) * a;
            }

            signedArea *= 0.5f;
            centroid.x /= (6 * signedArea);
            centroid.y /= (6 * signedArea);

            return centroid;
        }
    }
}