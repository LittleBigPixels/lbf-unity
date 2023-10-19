using System;
using UnityEngine;

namespace LBF
{
    public struct Bounds2D
    {
        public Vector2 Min { get; set; }
        public Vector2 Max { get; set; }

        public Vector2 HalfSize {
            get { return (Max - Min) * 0.5f; }
        }

        public Vector2 Size {
            get { return (Max - Min); }
        }

        public Bounds2D(Vector2 corner1, Vector2 corner2)
        {
            Min = new Vector2(Mathf.Min(corner1.x, corner2.x), Mathf.Min(corner1.y, corner2.y));
            Max = new Vector2(Mathf.Max(corner1.x, corner2.x), Mathf.Max(corner1.y, corner2.y));
        }

        public Bounds2D(Vector2 center, float width, float height)
        {
            Min = new Vector2(center.x - width / 2, center.y - height / 2);
            Max = new Vector2(center.x + width / 2, center.y + height / 2);
        }

        public Vector2 Center()
        {
            return 0.5f * (Min + Max);
        }

        public void Join(Bounds2D b)
        {
            Min = new Vector2(Mathf.Min(Min.x, b.Min.x), Mathf.Min(Min.y, b.Min.y));
            Max = new Vector2(Mathf.Max(Max.x, b.Max.x), Mathf.Max(Max.y, b.Max.y));
        }

        public void Encapsulate(Vector2 v)
        {
            Min = new Vector2(Mathf.Min(Min.x, v.x), Mathf.Min(Min.y, v.y));
            Max = new Vector2(Mathf.Max(Max.x, v.x), Mathf.Max(Max.y, v.y));
        }

        public void Encapsulate(Vector3 v)
        {
            throw new Exception("Bounds2D.Encapsulate doesn't accept Vector3 as a parameter. Do you want to pass a Vector2 instead?");
        }

        public void Extends(float extent)
        {
            Min = Min - Vector2.one * extent;
            Max = Max + Vector2.one * extent;
        }

        public bool Intersects(Bounds2D bounds)
        {
            return Bounds2D.Intersects(this, bounds);
        }

        public bool Contains(Vector2 pos)
        {
            return Contains(this, pos);
        }

        public static bool Contains(Bounds2D a, Vector2 pos)
        {
            return
                (a.Min.x <= pos.x && a.Max.x >= pos.x &&
                 a.Min.y <= pos.y && a.Max.y >= pos.y);
        }

        public static bool Intersects(Bounds2D a, Bounds2D b)
        {
            return
                !(a.Min.x >= b.Max.x || a.Max.x <= b.Min.x ||
                  a.Min.y >= b.Max.y || a.Max.y <= b.Min.y);
        }
    }
}
