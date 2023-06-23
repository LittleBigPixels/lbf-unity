using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LBF.Helpers
{
    public static class GizmosExtensions
    {
        public static void DrawPolygon3D(List<Vector3> points)
        {
            for (int i = 0; i < points.Count; i++)
                Gizmos.DrawLine(points[i], points[(i + 1) % points.Count]);
        }

        public static void DrawCurve3D(List<Vector3> points)
        {
            for (int i = 1; i < points.Count; i++)
                Gizmos.DrawLine(points[i - 1], points[i]);
        }

        public static void DrawArrow(Vector3 from, Vector3 to)
        {
            DrawArrow(from, to, Vector3.Distance(from, to) * 0.1f);
        }

        public static void DrawArc(Vector3 from, Vector3 to)
        {
            var dist = Vector2.Distance(from.XZ(), to.XZ());
            var stepDist = 8;
            var heigthMax = dist * 0.3f;

            var points = new List<Vector3>();
            var nPoints = (int)(dist / stepDist) + 5;
            for (int i = 0; i < nPoints; i++)
            {
                var k = i * 1.0f / (nPoints - 1);
                var heightCoef = k - k * k;
                points.Add(Vector3.Lerp(from, to, k) + Vector3.up * heightCoef * heigthMax);
            }

            for (int i = 1; i < points.Count; i++)
                Gizmos.DrawLine(points[i - 1], points[i]);
        }

        public static void DrawArrow(Vector3 from, Vector3 to, float headSize)
        {
            if (from == to) return;

            Gizmos.DrawLine(from, to);

            var dir = to - from;
            dir.Normalize();
            var ortho = Vector3.zero;
            if (dir != Vector3.up)
                ortho = Vector3.Cross(dir, Vector3.up);
            else
                ortho = Vector3.Cross(dir, Vector3.right);

            ortho.Normalize();
            var ortho2 = Vector3.Cross(dir, ortho);

            //Draw circle
            var nSide = 3;
            var points = new Vector3[nSide];
            var circleCenter = to - dir * headSize;
            for (int i = 0; i < nSide; i++)
            {
                var angle = i * 2 * Mathf.PI / nSide;
                points[i] = circleCenter + headSize * Mathf.Cos(angle) * ortho + headSize * Mathf.Sin(angle) * ortho2;
            }

            for (int i = 0; i < nSide; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % nSide]);
                Gizmos.DrawLine(points[i], to);
            }
        }

        public static void DrawLabel(Vector3 position, String text, Color color, int fontSize) 
        {
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = color;
            style.fontSize = fontSize;
            Handles.Label(position, text, style);
#endif
        }

        public static void DrawLabel(Vector3 position, String text)
        {
#if UNITY_EDITOR
            DrawLabel(position, text, Gizmos.color, 12);
#endif
        }
    }
}