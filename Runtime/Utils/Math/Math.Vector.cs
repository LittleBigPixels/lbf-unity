using LBF.Helpers;
using UnityEngine;

namespace LBF.Math
{
    public static partial class Math
    {
        public static float SlopeGradient(Vector3 from, Vector3 to)
        {
            float dxz = Vector2.Distance(to.XZ(), from.XZ());
            float dy = (to.y - from.y);
            return dxz / dy;
        }

        public static float SlopeAngle(Vector3 from, Vector3 to)
        {
            float dxz = Vector2.Distance(to.XZ(), from.XZ());
            float dy = (to.y - from.y);
            return (float)System.Math.Atan2(dy, dxz);
        }

        public static float SlopeAngleDegree(Vector3 from, Vector3 to)
        {
            float dx = Vector2.Distance(to.XZ(), from.XZ());
            float dy = (to.y - from.y);
            return (float)System.Math.Atan2(dy, dx) * Mathf.Rad2Deg;
        }

        public static float HorizontalDistance(Vector3 from, Vector3 to)
        {
            return Vector2.Distance(to.XZ(), from.XZ());
        }

        public static bool RaySphereIntersect(Ray ray, Vector3 center, float radius)
        {
            Vector3 relCenter = center - ray.origin;
            Vector3 proj = Vector3.Project(relCenter, ray.direction);
            return Vector3.SqrMagnitude(proj - relCenter) < radius * radius;
        }

        public static (int, int, float) GetClosestOnSegments(Vector3 position, Vector3[] points)
        {
            float bestDistanceSq = float.MaxValue;
            int bestIndex = -1;
            float bestK = 0;
            for (int i = 1; i < points.Length; i++)
            {
                Vector3 proj = ProjectPointSegment(position, points[i - 1], points[i]);
                float distSq = Vector3.SqrMagnitude(proj - position);
                if (distSq < bestDistanceSq)
                {
                    bestK = Vector3.Distance(proj, points[i - 1]) / Vector3.Distance(points[i - 1], points[i]);
                    bestIndex = i - 1; 
                    bestDistanceSq = distSq;
                }
            }

            return (bestIndex, bestIndex + 1, bestK);
        }

        public static Vector3 ProjectPointSegment(Vector3 position, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineDir = lineEnd - lineStart;
            if (Vector3.Dot(lineDir, position - lineStart) <= 0) return lineStart;
            if (Vector3.Dot(lineDir, position - lineEnd) >= 0) return lineEnd;

            Vector3 relativePosition = position - lineStart;
            float lineLength = lineDir.magnitude;
            lineDir = lineDir / lineLength;

            float distance = relativePosition.magnitude;

            float dot = Vector3.Dot(relativePosition, lineDir) / lineLength;

            float k = Math.Clamp01(dot);
            return lineStart + k * (lineEnd - lineStart);
        }
    }

    public static partial class Math
    {
        //From: https://www.habrador.com/tutorials/math/5-line-line-intersection/
        public static bool SegmentsIntersects(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            bool isIntersecting = false;

            float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

            //Make sure the denominator is > 0, if so the lines are parallel
            if (denominator == 0)
                return isIntersecting;
            
            float u_a = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
            float u_b = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

            //Is intersecting if u_a and u_b are between 0 and 1
            if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
            {
                isIntersecting = true;
            }

            return isIntersecting;
        }

        public static (int, int, float) GetClosestOnSegments(Vector2 position, Vector2[] points)
        {
            float bestDistanceSq = float.MaxValue;
            int bestIndex = -1;
            float bestK = 0;
            for (int i = 1; i < points.Length; i++)
            {
                Vector2 proj = ProjectPointSegment(position, points[i - 1], points[i]);
                float distSq = Vector2.SqrMagnitude(proj - position);
                if (distSq > bestDistanceSq)
                    continue;
                
                bestK = Vector2.Distance(proj, points[i - 1]) / Vector2.Distance(points[i - 1], points[i]);
                bestIndex = i - 1;
                bestDistanceSq = distSq;
            }

            return (bestIndex, bestIndex + 1, bestK);
        }

        public static float ProjectPointSegmentGetInterpolator(Vector2 position, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 lineDir = lineEnd - lineStart;
            if (Vector2.Dot(lineDir, position - lineStart) <= 0) return 0;
            if (Vector2.Dot(lineDir, position - lineEnd) >= 0) return 1;

            Vector2 relativePosition = position - lineStart;
            float lineLengthSq = Vector2.SqrMagnitude(lineDir);
            float dot = Vector2.Dot(relativePosition, lineDir) / lineLengthSq;

            return Clamp01(dot);
        }

        public static Vector2 ProjectPointSegment(Vector2 position, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 lineDir = lineEnd - lineStart;
            if (Vector2.Dot(lineDir, position - lineStart) <= 0) return lineStart;
            if (Vector2.Dot(lineDir, position - lineEnd) >= 0) return lineEnd;

            Vector2 relativePosition = position - lineStart;
            float lineLength = lineDir.magnitude;
            lineDir = lineDir / lineLength;

            float dot = Vector2.Dot(relativePosition, lineDir) / lineLength;

            float k = Clamp01( dot ); 
            return lineStart + k * (lineEnd - lineStart);
        }

        public static Vector2 Project(Vector2 v, Vector2 direction)
        {
            return Vector3.Project(v.FromXZ(), direction.FromXZ()).XZ();
        }

        public static Vector2 Map(float inMin, float inMax, Vector2 outMin, Vector2 outMax, float x)
        {
            if (inMin == inMax)
                return outMin;
            return outMin + (outMax - outMin) * (x - inMin) / (inMax - inMin);
        }
    }
}
