using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    public class CircleBoundary : IBoundaryQuery
    {
        public Vector2[] Vertices { get; }
        public Bounds2D BoundingBox => new Bounds2D(m_center, m_radius * 2, m_radius * 2);
        private Vector2 m_center;
        private float m_radius;
        private float m_radiusSquared;

        public CircleBoundary(Vector2 center, float radius)
        {
            m_center = center;
            m_radius = radius;
            m_radiusSquared = radius * radius;

            var pointCount = (int)(m_radius / 5) + 6;
            Vertices = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                var angle = Math.Map(0, pointCount, 0, Mathf.PI * 2, i);
                Vertices[i] = m_center + m_radius * Vector2.right.Rotate(angle);
            }
        }

        public bool Contains(Vector2 point)
        {
            return (point - m_center).sqrMagnitude <= m_radiusSquared;
        }

        public Helpers.BoundaryIntersection Raycast(Ray2D ray)
        {
            Vector2 rayDir = ray.direction.normalized;
            Vector2 rayToCenter = m_center - ray.origin;

            //Find t0 where the ray is the closest to the center
            float projection = Vector2.Dot(rayToCenter, rayDir);
            float t0 = projection;
            Vector2 rayClosest = ray.origin + t0 * rayDir;

            float closestDistanceSq = (m_center - rayClosest).sqrMagnitude;

            if (closestDistanceSq > m_radiusSquared)
                return new Helpers.BoundaryIntersection() { SegmentIndex = -1 };

            float d = Mathf.Sqrt(m_radiusSquared - closestDistanceSq);
            float t1 = t0 + d;
            float t2 = t0 - d;

            if (t1 < 0 && t2 < 0)
                return new Helpers.BoundaryIntersection() { SegmentIndex = -1 };

            float t = t1;
            if (t1 < 0 || (t2 > 0 && t1 < t2))
                t = t2;
            return new Helpers.BoundaryIntersection() { SegmentIndex = 0, Intersection = ray.origin + t * rayDir };
        }
    }
}