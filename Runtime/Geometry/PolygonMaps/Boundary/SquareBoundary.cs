using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    public class SquareBoundary : IBoundaryQuery
    {
        public Vector2[] Vertices { get; }
        public Bounds2D BoundingBox => m_boundingBox;

        private readonly Vector2 m_center;
        private readonly float m_width;
        private readonly Bounds2D m_boundingBox;

        public SquareBoundary(Vector2 center, float width)
        {
            m_center = center;
            m_width = width;
            m_boundingBox = new Bounds2D(center, width, width);
            Vertices = new Vector2[]
            {
                new Vector2(-width * 0.5f, -width * 0.5f),
                new Vector2(-width * 0.5f, +width * 0.5f),
                new Vector2(+width * 0.5f, +width * 0.5f),
                new Vector2(+width * 0.5f, -width * 0.5f),
            };
        }

        public bool Contains(Vector2 point)
        {
            return m_boundingBox.Contains(point);
        }

        public Helpers.BoundaryIntersection Raycast(Ray2D ray)
        {
            return Helpers.RaycastBoundary(Vertices, ray);
        }
    }
}