using System.Collections.Generic;
using LBF.Helpers;
using LBF.Math.Geometry.Polygon.ThirdParty;
using UnityEngine;
using UnityEngine.Profiling;
using Path = System.Collections.Generic.List<LBF.Math.Geometry.Polygon.ThirdParty.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<LBF.Math.Geometry.Polygon.ThirdParty.IntPoint>>;

namespace LBF.Math.Geometry.Polygon
{
    public static class PolygonHelper
    {
        const float ClipperScale = 100000;

        public static Polygon Transform(Polygon polygon, Transform transform)
        {
            Vector2[] newPoints = new Vector2[polygon.Points.Length];
            for (int i = 0; i < polygon.Points.Length; i++)
                newPoints[i] = transform.TransformPoint(((Vector2)polygon.Points[i]).FromXZ()).XZ();

            return new Polygon() { Points = newPoints };
        }

        public static Polygon InverseTransform(Polygon polygon, Transform transform)
        {
            Vector2[] newPoints = new Vector2[polygon.Points.Length];
            for (int i = 0; i < polygon.Points.Length; i++)
                newPoints[i] = transform.InverseTransformPoint(((Vector2)polygon.Points[i]).FromXZ()).XZ();

            return new Polygon() { Points = newPoints };
        }

        public static Polygon FromCircle(Vector2 center, float radius, int nPoint = 16)
        {
            List<Vector2> points = new List<Vector2>(nPoint);
            for (int i = 0; i < nPoint; i++)
            {
                float angle = Mathf.PI * 2 * (float)i / nPoint;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                points.Add(center + dir * radius);
            }

            return new Polygon(points);
        }

        public static Polygon FromLine(Vector2 from, Vector2 to, float width)
        {
            List<Vector2> points = new List<Vector2>();
            Vector2 dir = to - from;
            Vector2 side = dir.normalized.OrthogonalLeft();

            points.Add(from - side * width);
            points.Add(to - side * width);
            points.Add(to + side * width);
            points.Add(from + side * width);

            return new Polygon(points);
        }

        public static float DistanceToPolygon(Polygon polygon, Vector2 position)
        {
            if (Contains(polygon, position))
                return 0;

            return DistanceToBorder(polygon, position);
        }

        public static float DistanceToBorder(Polygon polygon, Vector2 position)
        {
            float dist = float.MaxValue;
            for (int i = 1; i <= polygon.Points.Length; i++)
            {
                Vector2 p1 = polygon.Points[i % polygon.Points.Length];
                Vector2 p2 = polygon.Points[i - 1];

                var closest = Math.ProjectPointSegment(position, p1, p2);
                float distSq = Vector2.SqrMagnitude(position - closest);
                if (distSq < dist * dist)
                    dist = Mathf.Sqrt(distSq);
            }

            return dist;
        }

        public static Vector2 ClosestOnPolygon(Polygon polygon, Vector2 position)
        {
            float dist = float.MaxValue;
            Vector2 bestPoint = Vector2.zero;
            for (int i = 1; i <= polygon.Points.Length; i++)
            {
                Vector2 p1 = polygon.Points[i % polygon.Points.Length];
                Vector2 p2 = polygon.Points[i - 1];

                var closest = Math.ProjectPointSegment(position, p1, p2);
                float distSq = Vector2.SqrMagnitude(position - closest);
                if (distSq < dist * dist)
                {
                    dist = Mathf.Sqrt(distSq);
                    bestPoint = closest;
                }
            }

            return bestPoint;
        }

        public static void ClosestOnPolygon(Polygon polygon, Vector2 input, out Vector2 position, out Vector2 normal)
        {
            float dist = float.MaxValue;
            Vector2 bestPoint = Vector2.zero;
            Vector2 bestPointDirection = Vector2.zero;
            for (int i = 1; i <= polygon.Points.Length; i++)
            {
                Vector2 p1 = polygon.Points[i % polygon.Points.Length];
                Vector2 p2 = polygon.Points[i - 1];

                var closest = Math.ProjectPointSegment(input, p1, p2);
                float distSq = Vector2.SqrMagnitude(input - closest);
                if (distSq < dist * dist)
                {
                    dist = Mathf.Sqrt(distSq);
                    bestPoint = closest;
                    bestPointDirection = VectorExtensions.OrthogonalLeft( (Vector2)(p1 - p2).normalized );
                }
            }

            position = bestPoint;
            normal = bestPointDirection;
        }

        public static bool Contains(Polygon polygon, Vector2 point)
        {
            //To determine if the polygon contains the point, count the number of time an horizontal line going through the point cross the polygon.
            //If the result is odd, then the point is inside the polygon.
            //
            //Here, for every edge of the polygon, find if it cross the horizontal line, and if yes, if it cross it to the left of the point

            int nEdges = 0; //Count the number of time the x line cross an edge
            for (int i = 1; i <= polygon.Points.Length; i++)
            {
                Vector2 p1 = polygon.Points[i % polygon.Points.Length];
                Vector2 p2 = polygon.Points[i - 1];

                if (Mathf.Max((float)p1.y, p2.y) >= point.y && Mathf.Min((float)p1.y, p2.y) < point.y)
                {
                    float x = Math.Map(p1.y, p2.y, p1.x, p2.x, point.y);
                    if (x < point.x) nEdges++;
                }
            }

            if (nEdges % 2 == 1)
                return true;
            return false;
        }

        public static Vector2 PseudoCenter(Polygon polygon)
        {
            if (polygon.Points.Length == 0) return Vector2.zero;

            Vector2 centerAcc = Vector2.zero;
            foreach (Vector2 p in polygon.Points)
                centerAcc += p;

            return centerAcc / polygon.Points.Length;
        }

        public static Vector2 Centroid(Vector2[] polygonPoints)
        {
            Vector2 centroid = Vector2.zero;
            float signedArea = 0;

            for (int i = 1; i <= polygonPoints.Length; i++)
            {
                Vector2 p1 = polygonPoints[i % polygonPoints.Length];
                Vector2 p2 = polygonPoints[i - 1];

                float a = p1.x * p2.y - p2.x * p1.y;
                signedArea += a;
                centroid.x += (p1.x + p2.x) * a;
                centroid.y += (p1.y + p2.y) * a;
            }

            signedArea = signedArea * 0.5f;
            return centroid / (6 * signedArea);
        }

        public static Vector2 Centroid(Polygon polygon)
        {
            return Centroid((Vector2[])polygon.Points);
        }

        public static Polygon Union(Polygon p1, Polygon p2)
        {
            return Union(new Polygon[] { p1, p2 });
        }

        public static Polygon Union(IEnumerable<Polygon> polygons)
        {
            Paths paths = new Paths();
            foreach (Polygon polygon in polygons)
                paths.Add(ToClipperPath(polygon));

            Paths solution = new Paths();
            Clipper c = new Clipper();
            c.AddPaths(paths, PolyType.ptSubject, true);
            c.Execute(ClipType.ctUnion, solution, PolyFillType.pftPositive);

            //TODO: check if always right
            Path outerPath = solution[0];

            return FromClipperPath(outerPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygons"></param>
        /// <returns></returns>
        public static Polygon[] UnionComplex(IEnumerable<Polygon> polygons)
        {
            Paths paths = new Paths();
            foreach (Polygon polygon in polygons)
                paths.Add(ToClipperPath(polygon));

            Paths solution = new Paths();
            Clipper c = new Clipper();
            c.AddPaths(paths, PolyType.ptSubject, true);
            c.Execute(ClipType.ctUnion, solution, PolyFillType.pftPositive);

            Polygon[] resultPolygons = new Polygon[solution.Count];
            for (int i = 0; i < resultPolygons.Length; i++)
                resultPolygons[i] = FromClipperPath(solution[i]);

            return resultPolygons;
        }

        /// <summary>
        /// Determine if the union of several polygons create a continuous region
        /// </summary>
        /// <returns></returns>
        public static bool IsContinuous(IEnumerable<Polygon> polygons)
        {
            Paths paths = new Paths();
            foreach (Polygon polygon in polygons)
                paths.Add(ToClipperPath(polygon));

            PolyTree polyTree = new PolyTree();
            Clipper c = new Clipper();
            c.AddPaths(paths, PolyType.ptSubject, true);
            c.Execute(ClipType.ctUnion, polyTree, PolyFillType.pftPositive);

            if (polyTree.ChildCount > 1)
                return false;

            return true;
        }

        public static bool Intersects(Polygon p1, Polygon p2)
        {
            if (p1.Bounds.Intersects(p2.Bounds) == false) return false;
            
            Profiler.BeginSample("Polygon.IntersectsBruteforce");
            bool intersect = IntersectsBruteforce(p1, p2);
            Profiler.EndSample();

            return intersect;
        }

        public static bool IntersectsBruteforce(Polygon p1, Polygon p2)
        {        
            for (int i = 0; i < p1.Points.Length; i++)
            {
                for (int j = 0; j < p2.Points.Length; j++)
                {
                    if (IntersectSegments(
                        p1.Points[i], p1.Points[(i + 1) % p1.Points.Length],
                        p2.Points[j], p2.Points[(j + 1) % p2.Points.Length]))
                        return true;
                }
            };

            for (int i = 0; i < p1.Points.Length; i++)
                if (Contains(p2, p1.Points[i])) return true;
            for (int i = 0; i < p2.Points.Length; i++)
                if (Contains(p1, p2.Points[i])) return true;

            return false;
        }

        public static bool IntersectSegments(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            Vector2 cmP = new Vector2(C.x - A.x, C.y - A.y);
            Vector2 r = new Vector2(B.x - A.x, B.y - A.y);
            Vector2 s = new Vector2(D.x - C.x, D.y - C.y);

            float cmPxr = cmP.x * r.y - cmP.y * r.x;
            float cmPxs = cmP.x * s.y - cmP.y * s.x;
            float rxs = r.x * s.y - r.y * s.x;

            if (cmPxr == 0f)
            {
                // Lines are collinear, and so intersect if they have any overlap

                return ((C.x - A.x < 0f) != (C.x - B.x < 0f))
                    || ((C.y - A.y < 0f) != (C.y - B.y < 0f));
            }

            if (rxs == 0f)
                return false; // Lines are parallel.

            float rxsr = 1f / rxs;
            float t = cmPxs * rxsr;
            float u = cmPxr * rxsr;

            return (t >= 0f) && (t <= 1f) && (u >= 0f) && (u <= 1f);
        }

        public static bool IntersectSegeentsStrict(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            Vector2 CmP = new Vector2(C.x - A.x, C.y - A.y);
            Vector2 r = new Vector2(B.x - A.x, B.y - A.y);
            Vector2 s = new Vector2(D.x - C.x, D.y - C.y);

            float CmPxr = CmP.x * r.y - CmP.y * r.x;
            float CmPxs = CmP.x * s.y - CmP.y * s.x;
            float rxs = r.x * s.y - r.y * s.x;

            if (CmPxr == 0f)
            {
                // Lines are collinear, and so intersect if they have any overlap

                return ((C.x - A.x < 0f) != (C.x - B.x < 0f))
                    || ((C.y - A.y < 0f) != (C.y - B.y < 0f));
            }

            if (rxs == 0f)
                return false; // Lines are parallel.

            float rxsr = 1f / rxs;
            float t = CmPxs * rxsr;
            float u = CmPxr * rxsr;

            return (t > 0f) && (t < 1f) && (u > 0f) && (u < 1f);
        }

        public static List<Vector2> DistributePoints(Polygon polygon, float step)
        {
            List<Vector2> list = new List<Vector2>();

            float currentDistance = 0;
            int currentPointIdx = 1;
            float currentSegmentStartDistance = 0;
            float currentSegmentEndDistance = Vector2.Distance(polygon.Points[1], polygon.Points[0]);

            while (true)
            {
                if (currentDistance > currentSegmentEndDistance)
                {
                    currentPointIdx++;
                    if (currentPointIdx > polygon.Points.Length)
                        break;

                    currentSegmentStartDistance = currentSegmentEndDistance;
                    currentSegmentEndDistance = currentSegmentStartDistance + Vector2.Distance(polygon.Points[currentPointIdx % polygon.Points.Length], polygon.Points[currentPointIdx - 1]);
                    continue;
                }

                Vector2 position = Math.Map(
                    currentSegmentStartDistance, currentSegmentEndDistance, 
                    polygon.Points[currentPointIdx - 1], polygon.Points[currentPointIdx % polygon.Points.Length], 
                    currentDistance);
                list.Add(position);

                currentDistance += step;

            }

            return list;
        }

        static List<IntPoint> ToClipperPath(Polygon polygon)
        {
            Path path = new List<IntPoint>();
            foreach (Vector2 point in polygon.Points)
                path.Add(new IntPoint(point.x * ClipperScale, point.y * ClipperScale));

            return path;
        }

        static Polygon FromClipperPath(Path path)
        {
            List<Vector2> points = new List<Vector2>();
            foreach (IntPoint pathPoint in path)
                points.Add(new Vector2(pathPoint.X / ClipperScale, pathPoint.Y / ClipperScale));

            return new Polygon(points);
        }
    }
}