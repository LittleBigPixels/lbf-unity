using UnityEngine;

namespace LBF.Math.Geometry.Curves
{
    public struct CurveControlPoint3D
    {
        public readonly Vector3 Position;
        public readonly Vector3 Direction;
        
        public CurveControlPoint3D( Vector3 position, Vector3 direction ) {
            Position = position;
            Direction = direction;
        }
    }

    public struct CurveControlPoint2D
    {
        public readonly Vector2 Position;
        public readonly Vector2 Direction;
        
        public CurveControlPoint2D( Vector2 direction, Vector2 position ) {
            Direction = direction;
            Position = position;
        }
    }
}