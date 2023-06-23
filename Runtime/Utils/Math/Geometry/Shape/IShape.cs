using System;
using LBF.Helpers;
using LBF.Math.Geometry.Polygon;
using UnityEngine;

namespace LBF.Math.Geometry.Shape
{
    public interface IShape
    {
        int GetPointCount();
        void GetPoints(Vector2[] points);
    }

    public static class ShapeExtensions
    {
        public static Vector2[] GetPoints(this IShape shape)
        {
            if (shape == null) return new Vector2[0];
            Vector2[] points = new Vector2[shape.GetPointCount()];
            shape.GetPoints(points);
            return points;
        }

        public static Polygon.Polygon GetPolygon(this IShape shape, Transform transform)
        {
            if (shape == null) return new Polygon.Polygon();
            Vector2[] points = new Vector2[shape.GetPointCount()];
            shape.GetPoints(points);

            for (int i = 0; i < points.Length; i++)
                points[i] = transform.TransformPoint(points[i].FromXZ()).XZ();

            Polygon.Polygon polygon = new Polygon.Polygon();
            polygon.SetPointsDirect(points);

            return polygon;
        }
    }

    [Serializable]
    public class PolygonShape : IShape
    {
        public Polygon.Polygon Polygon;

        public int GetPointCount()
        {
            return Polygon.Points.Length;
        }

        public void GetPoints(Vector2[] points)
        {
            Debug.Assert(points.Length == Polygon.Points.Length);
            for (int i = 0; i < points.Length; i++)
                points[i] = Polygon.Points[i];
        }

        public Polygon.Polygon GetPolygon(Transform transform)
        {
            return PolygonHelper.Transform(Polygon, transform);
        }
    }

    [Serializable]
    public class CircleShape : IShape
    {
        public const int PointCount = 16;
        public float Radius;

        public int GetPointCount()
        {
            return PointCount;
        }

        public void GetPoints(Vector2[] points)
        {
            Debug.Assert(points.Length == PointCount);
            for (int i = 0; i < PointCount; i++)
            {
                float angle = Mathf.PI * 2 * (float)i / PointCount;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                points[i] = dir * Radius;
            }
        }
    }

    [Serializable]
    public class RectangleShape : IShape
    {
        public float Width;
        public float Height;

        public int GetPointCount()
        {
            return 4;
        }

        public void GetPoints(Vector2[] points)
        {
            Debug.Assert(points.Length == 4);
            points[0] = new Vector2(-0.5f * Width, -0.5f * Height);
            points[1] = new Vector2(+0.5f * Width, -0.5f * Height);
            points[2] = new Vector2(+0.5f * Width, +0.5f * Height);
            points[3] = new Vector2(-0.5f * Width, +0.5f * Height);
        }
    }

    [Serializable]
    public class RoundedRectangleShape : IShape
    {
        const int CornerPointCount = 12;

        public float Width;
        public float Height;
        public float CornerRadius;

        public int GetPointCount()
        {
            return (CornerPointCount + 2) * 4;
        }

        public void GetPoints(Vector2[] points)
        {
            Debug.Assert(points.Length == GetPointCount());

            float cornerRadius = Mathf.Min(CornerRadius, Height / 2, Width / 2);

            Vector2 topLeftCornerCenter = new Vector2(-Width * 0.5f + cornerRadius, Height * 0.5f - cornerRadius);
            Vector2 topRightCornerCenter = new Vector2(Width * 0.5f - cornerRadius, Height * 0.5f - cornerRadius);
            Vector2 bottomLeftCornerCenter = new Vector2(-Width * 0.5f + cornerRadius, -Height * 0.5f + cornerRadius);
            Vector2 bottomRightCornerCenter = new Vector2(Width * 0.5f - cornerRadius, -Height * 0.5f + cornerRadius);

            Vector2 topLeft = new Vector2(-Width * 0.5f + cornerRadius, Height * 0.5f);
            Vector2 topRight = new Vector2(Width * 0.5f - cornerRadius, Height * 0.5f);
            Vector2 bottomLeft = new Vector2(-Width * 0.5f + cornerRadius, -Height * 0.5f);
            Vector2 bottomRight = new Vector2(Width * 0.5f - cornerRadius, -Height * 0.5f);

            Vector2 leftTop = new Vector2(-Width * 0.5f, Height * 0.5f - cornerRadius);
            Vector2 leftBottom = new Vector2(-Width * 0.5f, -Height * 0.5f + cornerRadius);
            Vector2 rightTop = new Vector2(Width * 0.5f, Height * 0.5f - cornerRadius);
            Vector2 rightBottom = new Vector2(Width * 0.5f, -Height * 0.5f + cornerRadius);

            int iPoint = 0;

            points[iPoint++] = topLeft;
            points[iPoint++] = topRight;
            for (int i = 0; i < CornerPointCount; i++)
                points[iPoint++] = topRightCornerCenter + cornerRadius * Vector2.up.Rotate(-Mathf.PI * 0.5f * (i + 1.0f) / (CornerPointCount));

            points[iPoint++] = rightTop;
            points[iPoint++] = rightBottom;
            for (int i = 0; i < CornerPointCount; i++)
                points[iPoint++] = bottomRightCornerCenter + cornerRadius * Vector2.right.Rotate(-Mathf.PI * 0.5f * (i + 1.0f) / (CornerPointCount));

            points[iPoint++] = bottomRight;
            points[iPoint++] = bottomLeft;
            for (int i = 0; i < CornerPointCount; i++)
                points[iPoint++] = bottomLeftCornerCenter + cornerRadius * Vector2.down.Rotate(-Mathf.PI * 0.5f * (i + 1.0f) / (CornerPointCount));

            points[iPoint++] = leftBottom;
            points[iPoint++] = leftTop;
            for (int i = 0; i < CornerPointCount; i++)
                points[iPoint++] = topLeftCornerCenter + cornerRadius * Vector2.left.Rotate(-Mathf.PI * 0.5f * (i + 1.0f) / (CornerPointCount));
        }
    }
}