using UnityEngine;

namespace LBF.Geometry.Curves
{
    public interface ICurve
    {
        float Length { get; }

        CurveSamplePoint Start { get; }
        CurveSamplePoint End { get; }
        Bounds2D Bounds { get; }

        int GetLastIndexBefore(float time);
        
        Vector2 GetAtTime(float time);
        Vector2 GetAtDistance(float distance);

        CurveSamplePoint GetPointAtTime(float time);
        CurveSamplePoint GetPointAtDistance(float distance);

        CurveSamplePoint GetClosestPoint(Vector2 from);
        Vector2 GetClosestPosition(Vector2 from);
        int GetClosestIndex(Vector2 from);
    }
}