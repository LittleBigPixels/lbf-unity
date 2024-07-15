using System.Collections.Generic;
using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    public partial class PolygonMap
    {
        public static PolygonMap HexagonalGrid(int hexCount, float hexSize)
        {
            var points = new List<Vector2>();
            var k = Mathf.Sqrt(hexCount);
            var s = hexSize * 0.5f;
            var h = Mathf.Sqrt(3) * s * Vector2.right;
            var v = 1.5f * s * Vector2.up;
            var min = -v * k / 2 - h * k / 2;
            for (int i = 0; i < k; i++)
            for (int j = 0; j < k; j++)
                points.Add(min + i * h + j * v + 0.5f * h * (j % 2));
            var max = min + k * h + k * v + 0.5f * h;
            var boundary = new RectangleBoundary(Vector2.zero, 2 * max.x, 2 * max.y);

            return PolygonMap.FromPoints(points.ToArray(), boundary);
        }

        public static PolygonMap SquareGrid(int cellCount, float size, int seed = 0)
        {
            var random = new System.Random(seed);
            var points = new List<Vector2>();
            for (int i = 0; i < cellCount; i++)
                points.Add(new Vector2(
                    0.48f * random.NextFloat(-size, size),
                    0.48f * random.NextFloat(-size, size)));
            var boundary = new SquareBoundary(Vector2.zero, size);

            var triangleGraph = TriangleGraph.CreateDelaunay(points.ToArray());

            int nRelax = 20;
            for (int i = 0; i < nRelax; i++)
            {
                PolygonGraph.LoydRelaxation(triangleGraph, boundary, 1.5f);
                triangleGraph = TriangleGraph.CreateDelaunay(triangleGraph.Vertices);
            }

            PolygonGraph.LoydRelaxation(triangleGraph, boundary);
            return PolygonMap.FromPoints(triangleGraph.Vertices, boundary);
        }

        public static PolygonMap RectangleGrid(int cellCount, float width, float height, int seed)
        {
            var random = new System.Random(seed);
            var points = new List<Vector2>();
            for (int i = 0; i < cellCount; i++)
                points.Add(new Vector2(
                    0.48f * random.NextFloat(-width, width),
                    0.48f * random.NextFloat(-height, height)));
            var boundary = new RectangleBoundary(Vector2.zero, width, height);

            var triangleGraph = TriangleGraph.CreateDelaunay(points.ToArray());

            int nRelax = 20;
            for (int i = 0; i < nRelax; i++)
            {
                PolygonGraph.LoydRelaxation(triangleGraph, boundary, 1.5f);
                triangleGraph = TriangleGraph.CreateDelaunay(triangleGraph.Vertices);
            }

            PolygonGraph.LoydRelaxation(triangleGraph, boundary);
            return PolygonMap.FromPoints(triangleGraph.Vertices, boundary);
        }
    }
}