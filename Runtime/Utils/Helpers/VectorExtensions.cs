using UnityEngine;

namespace LBF.Helpers
{
    public static class VectorExtensions
    {
        public static float CrossDet(Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }

        public static Vector2 XY(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector2 XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector3 FromXZ(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        public static Vector3 FromHorizontal(this Vector2 v, float height)
        {
            return new Vector3(v.x, height, v.y);
        }

        public static Vector3 ReplaceHeight(this Vector3 v, float height)
        {
            return new Vector3(v.x, height, v.z);
        }

        public static Vector3 Orthogonal(this Vector3 v)
        {
            if (v == Vector3.up || v == Vector3.down) return Vector3.Cross(v, Vector3.forward);
            return Vector3.Cross(v, Vector3.up);
        }

        public static float HorizontalDistance(Vector3 v1, Vector3 v2)
        {
            return Vector2.Distance(v1.XZ(), v2.XZ());
        }

        public static Vector2 OrthogonalLeft(this Vector2 v)
        {
            return new Vector2(-v.y, v.x);
        }

        public static Vector2 OrthogonalRight(this Vector2 v)
        {
            return -OrthogonalLeft(v);
        }

        public static Vector2 Rotate(this Vector2 v, float angle)
        {
            return new Vector2(
                Mathf.Cos(angle) * v.x - Mathf.Sin(angle) * v.y,
                Mathf.Sin(angle) * v.x + Mathf.Cos(angle) * v.y);
        }

        public static Vector2 Clamp(this Vector2 v, Vector2 min ,Vector2 max)
        {
            return new Vector2(
                Mathf.Clamp(v.x, min.x, max.x),
                Mathf.Clamp(v.y, min.y, max.y));
        }
    }
}