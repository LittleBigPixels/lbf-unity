using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LBF.Geometry.PolygonMaps
{
    public static partial class Helpers
    {
        public struct BoundaryIntersection
        {
            public int SegmentIndex;
            public Vector2 Intersection;
        }

        //Find the intersection of ray cast from the inside of a convex boundary
        //TODO: rework function
        public static BoundaryIntersection RaycastBoundary(Vector2[] boundary, Ray2D ray)
        {
            BoundaryIntersection bestIntersection = new BoundaryIntersection();
            var bestDist = float.MaxValue;

            for (int i = 0; i < boundary.Length; i++)
            {
                var s1 = boundary[i];
                var s2 = boundary[(i + 1) % boundary.Length];

                //Check side (left/right) of s1 and s2 relative to the ray
                var det1 = VectorExtensions.CrossDet(ray.direction, s1 - ray.origin);
                var det2 = VectorExtensions.CrossDet(ray.direction, s2 - ray.origin);
                //Check side (front/back) of s1 and s2 relative to the ray
                var dot1 = Vector2.Dot(ray.direction, s1 - ray.origin);
                var dot2 = Vector2.Dot(ray.direction, s2 - ray.origin);

                //If both are on the same side (same sign), the segment can't intersect the ray
                if (det1 * det2 > 0) continue;
                if (dot1 < 0 && dot2 < 0) continue;

                //Remaining segment intersects
                var intersection = LineRayIntersectionAssumeExists(ray, s1, s2);
                var interpolator = (intersection - s1).magnitude / (s2 - s1).magnitude;

                var dist = Vector2.Distance(intersection, ray.origin);
                if (dist < bestDist)
                {
                    bestIntersection = new BoundaryIntersection()
                    {
                        SegmentIndex = i,
                        Intersection = intersection,
                    };
                    bestDist = dist;
                }
            }

            Debug.Assert(bestDist < float.MaxValue);
            if (bestDist < float.MaxValue == false)
                return new BoundaryIntersection() { SegmentIndex = -1 };

            return bestIntersection;
        }

        public static Vector2 LineRayIntersectionAssumeExists(Ray2D ray, Vector2 v1, Vector2 v2)
        {
            Vector2 rayStart = ray.origin;
            Vector2 rayDirection = ray.direction;

            Vector2 lineDirection = v2 - v1;

            float cross = VectorExtensions.CrossDet(rayDirection, lineDirection);

            Vector2 startPointDiff = v1 - rayStart;
            float t = VectorExtensions.CrossDet(startPointDiff, lineDirection) / cross;
            float u = VectorExtensions.CrossDet(startPointDiff, rayDirection) / cross;

            return rayStart + t * rayDirection;
        }
    }
}