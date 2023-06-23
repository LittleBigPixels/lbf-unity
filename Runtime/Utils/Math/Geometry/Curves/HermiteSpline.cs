using LBF.Helpers;
using UnityEngine;

namespace LBF.Math.Geometry.Curves
{
    public class HermiteSpline
    {
        public static void Sample2D(CurveControlPoint2D start, CurveControlPoint2D end, CurveSamplePoint[] results, float tension = 1.0f)
        {
            float dist = Vector3.Distance(start.Position, end.Position);
            float tangentCoef = tension * dist;

            float totalDistance = 0;
            for (int i = 0; i < results.Length; i++)
            {
                float t = i * 1.0f / (results.Length - 1);
                Vector2 position = HermiteInterpolation2D(start.Position, tangentCoef * start.Direction, end.Position, tangentCoef * end.Direction, t);
                Vector2 derivative = HermiteInterpolationDerivative2D(start.Position, tangentCoef * start.Direction, end.Position, tangentCoef * end.Direction, t);
                Vector2 tangent = derivative.normalized;

                if (i > 0)
                    totalDistance += Vector2.Distance(position, results[i - 1].Position);

                results[i].Position = position;
                results[i].Direction = tangent;
                results[i].Distance = totalDistance;
            }

            for (int i = 0; i < results.Length; i++)
                results[i].Time = results[i].Distance / totalDistance;
        }

        public static void Sample3D(CurveControlPoint3D start, CurveControlPoint3D end, CurveSamplePoint[] results, float tension = 1.0f)
        {
            float dist = Vector3.Distance(start.Position, end.Position);
            float tangentCoef = tension * dist;

            float totalDistance = 0;
            for(int i = 0; i < results.Length; i++)
            {
                float t = i * 1.0f / (results.Length - 1);
                Vector3 position = HermiteInterpolation3D(start.Position, tangentCoef * start.Direction, end.Position, tangentCoef * end.Direction, t);
                Vector3 derivative = HermiteInterpolationDerivative3D(start.Position, tangentCoef * start.Direction, end.Position, tangentCoef * end.Direction, t);
                Vector3 tangent = Vector3.Normalize(derivative);

                if (i > 0)
                    totalDistance += Vector2.Distance(position.XZ(), results[i - 1].Position);

                results[i].Position = position.XZ();
                results[i].Direction = tangent.XZ();
                results[i].Distance = totalDistance;
            }

            for (int i = 0; i < results.Length; i++)
                results[i].Time = results[i].Distance / totalDistance;
        }

        public static Vector2 HermiteInterpolation2D(Vector2 from, Vector2 fromDir, Vector2 to, Vector2 toDir, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float h1 = 2 * t3 - 3 * t2 + 1;
            float h2 = -2 * t3 + 3 * t2;
            float h3 = t3 - 2 * t2 + t;
            float h4 = t3 - t2;

            return from * h1 + to * h2 + fromDir * h3 + toDir * h4;
        }

        public static Vector3 HermiteInterpolation3D(Vector3 from, Vector3 fromDir, Vector3 to, Vector3 toDir, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float h1 = 2 * t3 - 3 * t2 + 1;
            float h2 = -2 * t3 + 3 * t2;
            float h3 = t3 - 2 * t2 + t;
            float h4 = t3 - t2;

            return from * h1 + to * h2 + fromDir * h3 + toDir * h4;
        }

        public static Vector2 HermiteInterpolationDerivative2D(Vector2 from, Vector2 fromDir, Vector2 to, Vector2 toDir, float t)
        {
            float t2 = t * t;
            float h1 = 6 * t2 - 6 * t;
            float h2 = -6 * t2 + 6 * t;
            float h3 = 3 * t2 - 4 * t + 1;
            float h4 = 3 * t2 - 2 * t;

            return from * h1 + to * h2 + fromDir * h3 + toDir * h4;
        }

        public static Vector3 HermiteInterpolationDerivative3D(Vector3 from, Vector3 fromDir, Vector3 to, Vector3 toDir, float t)
        {
            float t2 = t * t;
            float h1 = 6 * t2 - 6 * t;
            float h2 = -6 * t2 + 6 * t;
            float h3 = 3 * t2 - 4 * t + 1;
            float h4 = 3 * t2 - 2 * t;

            return from * h1 + to * h2 + fromDir * h3 + toDir * h4;
        }
    }
}