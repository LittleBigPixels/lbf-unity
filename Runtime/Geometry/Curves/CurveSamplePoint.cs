using UnityEngine;

namespace LBF.Geometry.Curves
{
    public struct CurveSamplePoint
    {
        public Vector2 Position;
        public Vector2 Direction;

        public float Time;
        public float Distance;

        public static CurveSamplePoint Lerp(CurveSamplePoint p1, CurveSamplePoint p2, float t)
        {
            return new CurveSamplePoint()
            {
                Position = Vector2.Lerp(p1.Position, p2.Position, t),
                Direction = Vector2.Lerp(p1.Direction, p2.Direction, t),
                Distance = Mathf.Lerp(p1.Distance, p2.Distance, t),
                Time = Mathf.Lerp(p1.Time, p2.Time, t),
            };
        }
    }
}
