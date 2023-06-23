using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LBF.Math.Geometry.Polygon
{
    [Serializable]
    public class Polygon
    {
        [SerializeField]
        Vector2[] m_points;
        public Vector2[] Points {
            get => m_points;
            set => SetPointsDirect(value);
        }

        [SerializeField]
        Bounds2D m_bounds;
        public Bounds2D Bounds => m_bounds;

        public Polygon()
        {
            m_points = new Vector2[0];
            m_bounds = new Bounds2D();
        }

        public Polygon(IEnumerable<Vector2> points)
        {
            SetPoints(points);
        }

        public Tuple<Vector2, Vector2> GetEdgeTo(int i)
        {
            return new Tuple<Vector2, Vector2>(
                m_points[(i - 1 + m_points.Length) % m_points.Length],
                m_points[(i + m_points.Length) % m_points.Length]);
        }

        public Vector2 GetVertex(int i)
        {
            return m_points[i % m_points.Length];
        }

        public void SetPointsDirect(Vector2[] points)
        {
            m_points = points;

            if (m_points.Length == 0)
            {
                m_bounds = new Bounds2D();
                return;
            }

            float signedArea = 0;
            for (int i = 0; i < m_points.Length; i++)
            {
                Vector2 p1 = m_points[i];
                Vector2 p2 = m_points[(i - 1 + m_points.Length) % m_points.Length];
                signedArea += (p2.x - p1.x) * (p2.y + p1.y);
            }

            if (signedArea < 0)
            {
                //Reverse the point array
                for (int i = 0; i < m_points.Length / 2; i++)
                    (m_points[i], m_points[m_points.Length - i - 1]) = (m_points[m_points.Length - i - 1], m_points[i]);
            }

            m_bounds = new Bounds2D(m_points[0], m_points[0]);
            foreach (Vector2 point in m_points)
                m_bounds.Encapsulate(point);
        }

        void SetPoints(IEnumerable<Vector2> points)
        {
            SetPointsDirect(points.ToArray());
        }

        public bool Contains(Vector2 point)
        {
            return PolygonHelper.Contains(this, point);
        }

        public bool IsClockWise()
        {
            float sum = 0;
            for (int i = 0; i < m_points.Length; i++)
            {
                Tuple<Vector2, Vector2> edge = GetEdgeTo(i);
                sum += (edge.Item2.x - edge.Item1.x) * (edge.Item2.y + edge.Item1.y);
            }

            return sum >= 0;
        }
    }
}
