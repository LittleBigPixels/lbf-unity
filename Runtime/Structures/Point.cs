using System;

namespace LBF.Structures
{
    public struct Point
    {
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", x, y);
        }

        public static bool operator ==(Point c1, Point c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(Point c1, Point c2)
        {
            return !c1.Equals(c2);
        }
        
        bool Equals( Point other ) => x == other.x && y == other.y;
        
        public override bool Equals( object obj ) => obj is Point other && Equals( other );
        public override int GetHashCode() => HashCode.Combine( x, y );
    }
}