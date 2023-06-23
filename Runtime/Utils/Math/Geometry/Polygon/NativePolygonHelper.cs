using Unity.Collections;
using UnityEngine;

namespace LBF.Math.Geometry.Polygon
{
    public static class NativePolygonHelper
    {
        public static bool Contains(NativeArray<Vector2> polygonPoints, Vector2 point)
        {
            return Contains(polygonPoints, 0, polygonPoints.Length, point);
        }

        public static bool Contains(NativeArray<Vector2> polygonPoints, int startIndex, int count, Vector2 point)
        {
            //To determine if the polygon contains the point, count the number of time an horizontal line going through the point cross the polygon.
            //If the result is odd, then the point is inside the polygon.
            //
            //Here, for every edge of the polygon, find if it cross the horizontal line, and if yes, if it cross it to the left of the point

            int nEdges = 0; //Count the number of time the x line cross an edge
            for (int i = 1; i <= count; i++)
            {
                var p1 = polygonPoints[startIndex + i % count];
                var p2 = polygonPoints[startIndex + i - 1];

                if (Mathf.Max(p1.y, p2.y) >= point.y && Mathf.Min(p1.y, p2.y) < point.y)
                {
                    var x = Math.Map(p1.y, p2.y, p1.x, p2.x, point.y);
                    if (x < point.x) nEdges++;
                }
            }

            if (nEdges % 2 == 1)
                return true;
            return false;
        }

        public static bool IntersectSegements(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            Vector2 CmP = new Vector2(c.x - a.x, c.y - a.y);
            Vector2 r = new Vector2(b.x - a.x, b.y - a.y);
            Vector2 s = new Vector2(d.x - c.x, d.y - c.y);

            float CmPxr = CmP.x * r.y - CmP.y * r.x;
            float CmPxs = CmP.x * s.y - CmP.y * s.x;
            float rxs = r.x * s.y - r.y * s.x;

            if (CmPxr == 0f)
            {
                // Lines are collinear, and so intersect if they have any overlap

                return ((c.x - a.x < 0f) != (c.x - b.x < 0f))
                    || ((c.y - a.y < 0f) != (c.y - b.y < 0f));
            }

            if (rxs == 0f)
                return false; // Lines are parallel.

            float rxsr = 1f / rxs;
            float t = CmPxs * rxsr;
            float u = CmPxr * rxsr;

            return (t >= 0f) && (t <= 1f) && (u >= 0f) && (u <= 1f);
        }
    }
}