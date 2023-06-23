using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LBF.Math.Geometry.Curves
{
    public class CurveHelper
    {
        public static CurveSamplePoint InterpolateCurveSamplePoint(CurveSamplePoint p1, CurveSamplePoint p2, float k)
        {
            CurveSamplePoint result = new CurveSamplePoint()
            {
                Position = Vector2.Lerp(p1.Position, p2.Position, k),
                Direction = Vector2.Lerp(p1.Direction, p2.Direction, k),
                Distance = Mathf.Lerp(p1.Distance, p2.Distance, k),
                Time = Mathf.Lerp(p1.Time, p2.Time, k),
            };

            return result;
        }

        /// <summary>
        /// Take several curves and merge their sample points, offsetting the distance and recomputing the time
        /// </summary>
        /// <param name="curveSamples"></param>
        /// <returns></returns>
        public static CurveSamplePoint[] MergeCurveSamples(IEnumerable<CurveSamplePoint[]> curveSamples)
        {
            float totalDistance = 0;
            foreach (CurveSamplePoint[] curveSample in curveSamples)
            {
                if (curveSample.Length == 0) continue;
                totalDistance += curveSample.Last().Distance - curveSample.First().Distance;
            }

            float runningDistance = 0;
            List<CurveSamplePoint> mergedPointList = new List<CurveSamplePoint>(curveSamples.Sum(a=> a.Length));
            foreach (CurveSamplePoint[] curveSample in curveSamples)
            {
                if (curveSample.Length == 0) continue;
                foreach (CurveSamplePoint point in curveSample)
                {
                    float relativeDistance = point.Distance - curveSample.First().Distance;
                    CurveSamplePoint newPoint = new CurveSamplePoint() {
                        Position = point.Position,
                        Direction = point.Direction,
                        Distance = relativeDistance + runningDistance,
                        Time = (relativeDistance + runningDistance) / totalDistance,
                    };
                    mergedPointList.Add(newPoint);
                }
                runningDistance += curveSample.Last().Distance - curveSample.First().Distance;
            }

            return mergedPointList.ToArray();
        }

        public static List<CurveSamplePoint> DistributePoints(ICurve path, float distance)
        {
            List<CurveSamplePoint> points = new List<CurveSamplePoint>();
            float nStep = Mathf.Ceil(path.Length / distance);
            float step = path.Length / nStep;

            points.Add(path.Start);
            for (int i = 1; i < nStep; i++)
            {
                float dist = i * step;
                points.Add(path.GetPointAtDistance(dist));
            }
            points.Add(path.End);

            return points;
        }

        public static List<CurveSamplePoint> DistributePointsStable(ICurve path, float distance)
        {
            List<CurveSamplePoint> points = new List<CurveSamplePoint>();
            float nStep = Mathf.Ceil(path.Length / distance);

            points.Add(path.Start);
            for (int i = 1; i < nStep; i++)
            {
                float dist = i * distance;
                points.Add(path.GetPointAtDistance(dist));
            }
            points.Add(path.End);

            return points;
        }

        public static void DistributePointsStable(CurveSamplePoint[] points, float distanceStep, List<CurveSamplePoint> results)
        {
            results.Clear();
            results.Add(points[0]);

            float curveDistance = points[points.Length - 1].Distance - points[0].Distance;
            if (curveDistance < distanceStep)
                return;            

            float nStep = Mathf.Ceil(curveDistance / distanceStep);
            float realDistanceStep = curveDistance / nStep;
            float timeStep = realDistanceStep / curveDistance;

            for (int i = 1; i < nStep; i++)
            {
                float time = i * timeStep;
                results.Add(FindPointAtTime(points, time));
            }
            results.Add(points[points.Length - 1]);
        }

        public static CurveSamplePoint FindPointAtTime(CurveSamplePoint[] points, float time)
        {
            float clampedTime = Mathf.Clamp01(time);
            for (int i = 1; i < points.Length; i++)
            {
                float epsilon = float.Epsilon;
                if (points[i].Time >= clampedTime - epsilon)
                {
                    float k = Math.Map(points[i - 1].Time, points[i].Time, 0, 1, clampedTime);
                    return InterpolateCurveSamplePoint(points[i - 1], points[i], k);
                }
            }

            return points.Last();
        }
    }
}
