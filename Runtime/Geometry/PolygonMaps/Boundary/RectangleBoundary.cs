using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    public class RectangleBoundary : IBoundaryQuery
    {
        public Vector2[] Vertices { get; }
        public Bounds2D BoundingBox => m_boundingBox;

        private readonly Vector2 m_center;
        private readonly float m_width;
        private readonly float m_height;
        private readonly Bounds2D m_boundingBox;

        public RectangleBoundary(Vector2 center, float width, float height)
        {
            m_center = center;
            m_width = width;
            m_height = height;
            
            m_boundingBox = new Bounds2D(center, width, height);
            Vertices = new Vector2[]
            {
                new Vector2(-width * 0.5f, -height * 0.5f),
                new Vector2(-width * 0.5f, +height * 0.5f),
                new Vector2(+width * 0.5f, +height * 0.5f),
                new Vector2(+width * 0.5f, -height * 0.5f),
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