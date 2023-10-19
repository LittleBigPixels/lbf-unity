using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    public static partial class Helpers
    {
        public static Vector2 Barycenter(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            return (v1 + v2 + v3) / 3;
        }

        //Note: from wikipedia, https://en.wikipedia.org/wiki/Circumcircle, Barycentric coordinates
        public static Vector2 Circumcenter(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            var a = (v3 - v2).sqrMagnitude;
            var b = (v3 - v1).sqrMagnitude;
            var c = (v2 - v1).sqrMagnitude;

            var ca = a * (b + c - a);
            var cb = b * (c + a - b);
            var cc = c * (a + b - c);

            return (ca * v1 + cb * v2 + cc * v3) / (ca + cb + cc);
        }        
    }
}