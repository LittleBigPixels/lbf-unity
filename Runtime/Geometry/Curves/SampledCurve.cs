using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LBF.Geometry.Curves
{
    public class SampledCurve : ICurve
    {
        readonly Bounds2D m_bounds;
        public Bounds2D Bounds => m_bounds;

        public CurveSamplePoint Start => Samples[0];
        public CurveSamplePoint End => Samples.Last();

        public float Length { get; }

        public CurveSamplePoint[] Samples { get; }

        public SampledCurve()
        {
            Samples = new[] {
                new CurveSamplePoint() { Time = 0 },
                new CurveSamplePoint() { Time = 1 },
            };

            Length = Samples.Last().Distance;

            m_bounds = new Bounds2D();
            foreach (CurveSamplePoint point in Samples)
                m_bounds.Encapsulate(point.Position);
        }

        public SampledCurve(CurveSamplePoint[] samplePoints)
        {
            Samples = samplePoints;
            if (Samples.Length == 0)
                Samples = new[] {
                    new CurveSamplePoint() { Time = 0 },
                    new CurveSamplePoint() { Time = 1 },
                };

            Length = Samples.Last().Distance;

            m_bounds = new Bounds2D();
            foreach (CurveSamplePoint point in Samples)
                m_bounds.Encapsulate(point.Position);
        }

        public SampledCurve(IEnumerable<CurveSamplePoint> samplePoints)
        {
            Samples = samplePoints.ToArray();
            if (Samples.Length == 0)
                Samples = new[] {
                    new CurveSamplePoint() { Time = 0 },
                    new CurveSamplePoint() { Time = 1 },
                };

            Length = Samples.Last().Distance;

            m_bounds = new Bounds2D();
            foreach (CurveSamplePoint point in Samples)
                m_bounds.Encapsulate(point.Position);
        }

        public Vector2 GetAtDistance(float distance)
        {
            float time = distance / Length;
            return GetAtTime(time);
        }

        public Vector2 GetAtTime(float time)
        {
            CurveSamplePoint point = GetPointAtTime(time);
            return point.Position;
        }

        public CurveSamplePoint GetClosestPoint(Vector2 from)
        {
            CurveSamplePoint bestPoint = Samples[0];
            float bestDistanceSq = float.MaxValue;
            for (int i = 1; i < Samples.Length; i++)
            {
                Vector2 proj = Math.ProjectPointSegment(from, Samples[i].Position, Samples[i - 1].Position);
                float distSq = Vector2.SqrMagnitude(proj - from);
                if (distSq < bestDistanceSq)
                {
                    float k = Vector2.Distance(proj, Samples[i - 1].Position) / Vector2.Distance(Samples[i].Position, Samples[i - 1].Position);
                    bestPoint = CurveSamplePoint.Lerp(Samples[i - 1], Samples[i], k);
                    bestDistanceSq = distSq;
                }
            }

            return bestPoint;
        }

        public Vector2 GetClosestPosition(Vector2 from)
        {
            return GetClosestPoint(from).Position;
        }

        public int GetClosestIndex(Vector2 from)
        {
            int bestIndex = -1;
            float bestDistanceSq = float.MaxValue;
            for (int i = 0; i < Samples.Length; i++)
            {
                float distSq = Vector2.SqrMagnitude(from - Samples[i].Position);
                if (distSq < bestDistanceSq)
                {
                    bestIndex = i;
                    bestDistanceSq = distSq;
                }
            }

            return bestIndex;
        }

        public CurveSamplePoint GetPointAtDistance(float distance)
        {
            float time = distance / Length;
            return GetPointAtTime(time);
        }

        public CurveSamplePoint GetPointAtTime(float time)
        {
            (int idx1, int idx2, float k) = GetInterpolationIndicesAndFractionAtTime(time);
            return CurveSamplePoint.Lerp(Samples[idx1], Samples[idx2], k);
        }

        public (int, int, float) GetInterpolationIndicesAndFractionAtTime(float time)
        {
            int index = GetLastIndexBefore(time);

            if (index == Samples.Length - 1)
                return (index, index, 0);

            float k = Math.MapFrom(Samples[index].Time, Samples[index + 1].Time, time);
            return (index, index + 1, k);
        }

        public int GetLastIndexBefore(float time)
        {
            if (Samples.Length == 1) return 0;
            if (time <= 0) return 0;
            if (time >= 1) return Samples.Length - 1;

            int predictedIndex = (int)(Samples.Length * time);

            bool searchUp = Samples[predictedIndex].Time < time;

            if (searchUp)
            {
                for (int i = predictedIndex; i < Samples.Length - 1; i++)
                    if (Samples[i].Time <= time && Samples[i + 1].Time > time) return i;
                return Samples.Length - 1;
            }
            else
            {
                if (predictedIndex == 0) return 0;
                for (int i = predictedIndex - 1; i > 0; i--)
                    if (Samples[i].Time <= time && Samples[i + 1].Time > time) return i;
                return 0;
            }
        }
    }
}
