using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace LBF.Structures
{
    public struct NativeSpatialGrid<T> : IDisposable, IEnumerable<T> where T : struct
    {
        public NativeArray<T> Data;

        readonly int m_countX;
        public int CountX => m_countX;

        readonly int m_countY;
        public int CountY => m_countY;

        readonly Vector2 m_min;
        public Vector2 Min => m_min;

        readonly Vector2 m_max;
        public Vector2 Max => m_max;

        public Vector2 Center => 0.5f * (m_max + m_min);

        public Vector2 Size => m_max - m_min;

        public Vector2 CellSize => new Vector2((m_max.x - m_min.x) / (m_countX), (m_max.y - m_min.y) / (m_countY));

        public IEnumerable<Point> Indices {
            get {
                for (int i = 0; i < m_countX; i++) for (int j = 0; j < m_countY; j++) yield return new Point(i, j);
            }
        }

        public T this[Vector2 position] {
            get => Get(position);
            set => Set(value, position);
        }

        public T this[int i, int j] {
            get => Get(i, j);
            set => Set(value, i, j);
        }

        public T this[Point p] {
            get => Get(p.x, p.y);
            set => Set(value, p.x, p.y);
        }

        public NativeSpatialGrid(Vector2 minCorner, Vector2 maxCorner, int nCellX, int nCellY, Allocator allocator = Allocator.Persistent)
        {
            m_min = new Vector2(
                System.Math.Min(minCorner.x, maxCorner.x),
                System.Math.Min(minCorner.y, maxCorner.y));
            m_max = new Vector2(
                System.Math.Max(minCorner.x, maxCorner.x),
                System.Math.Max(minCorner.y, maxCorner.y));

            m_countX = nCellX;
            m_countY = nCellY;

            Data = new NativeArray<T>(m_countX * m_countY, allocator, NativeArrayOptions.ClearMemory);
        }

        public void Dispose()
        {
            if (Data.IsCreated)
                Data.Dispose();
        }

        public T Get(int i, int j)
        {
            int index = GetLinearIndex(i, j);
            return Data[index];
        }

        public T Get(Vector2 position)
        {
            Point point = GetCellIndex(position);
            return Get(point.x, point.y);
        }

        public void Set(T value, int i, int j)
        {
            int index = GetLinearIndex(i, j);
            Data[index] = value;
        }

        public void Set(T value, Vector2 position)
        {
            Point point = GetCellIndex(position);
            Set(value, point.x, point.y);
        }

        public int GetLinearIndex(int i, int j)
        {
            return j * m_countX + i;
        }

        public int GetLinearIndex(Point p)
        {
            return GetLinearIndex(p.x, p.y);
        }

        public int GetLinearIndex(Vector2 position)
        {
            Point point = GetCellIndex(position);
            return GetLinearIndex(point.x, point.y);
        }

        public Point GetCellIndex(int linearIndex)
        {
            return new Point(linearIndex % m_countX, linearIndex / m_countX);
        }

        public Point GetCellIndex(Vector2 position)
        {
            float localX = position.x - m_min.x;
            float localY = position.y - m_min.y;

            if (CellSize.x == 0)
                return new Point(0, 0);
            if (CellSize.y == 0)
                return new Point(0, 0);

            int i = (int)UnityEngine.Mathf.FloorToInt(localX / CellSize.x);
            int j = (int)UnityEngine.Mathf.FloorToInt(localY / CellSize.y);

            if (i == CountX)
                i--;

            if (j == CountY)
                j--;

            if (Contains(i, j) == false)
                throw new ArgumentOutOfRangeException();

            return new Point(i, j);
        }

        public Point GetCellIndexBounded(Vector2 position)
        {
            position = Vector2.Max(m_min, position);
            position = Vector2.Min(m_max, position);

            float localX = position.x - m_min.x;
            float localY = position.y - m_min.y;

            if (CellSize.x == 0)
                return new Point(0, 0);
            if (CellSize.y == 0)
                return new Point(0, 0);

            int i = (int)UnityEngine.Mathf.FloorToInt(localX / CellSize.x);
            int j = (int)UnityEngine.Mathf.FloorToInt(localY / CellSize.y);
            i = System.Math.Min(System.Math.Max(i, 0), CountX - 1);
            j = System.Math.Min(System.Math.Max(j, 0), CountY - 1);

            return new Point(i, j);
        }

        public IEnumerable<Point> GetNeighbours4(Point p)
        {
            if (p.x != 0) yield return new Point(p.x - 1, p.y);
            if (p.x != CountX - 1) yield return new Point(p.x + 1, p.y);
            if (p.y != 0) yield return new Point(p.x, p.y - 1);
            if (p.y != CountY - 1) yield return new Point(p.x, p.y + 1);
        }

        public IEnumerable<Point> GetNeighbours8(Point p)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0) continue;
                    if (Contains(p.x + i, p.y + j)) yield return new Point(p.x + i, p.y + j);
                }
            }
        }

        public IEnumerable<Point> GetNeighbours24(Point p)
        {
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    if (i == 0 && j == 0) continue;
                    if (Contains(p.x + i,
                        p.y + j)) yield return new Point(p.x + i, p.y + j);
                }
            }
        }

        public IEnumerable<Point> GetIndices(Vector2 min, Vector2 max)
        {
            min = min.Clamp(m_min, m_max);
            max = max.Clamp(m_min, m_max);

            Point minIdx = GetCellIndex(min);
            Point maxIdx = GetCellIndex(max);

            for (int i = minIdx.x; i <= maxIdx.x; i++)
                for (int j = minIdx.y; j <= maxIdx.y; j++) yield return new Point(i, j);
        }

        public Vector2 GetCellCenter(Point p)
        {
            return GetCellCenter(p.x, p.y);
        }

        public Vector2 GetCellCenter(int i, int j)
        {
            return new Vector2(
                Min.x + CellSize.x * (0.5f + i),
                Min.y + CellSize.y * (0.5f + j));
        }

        public T GetInterpolated(Vector2 position, Func<T, T, float, T> interp)
        {
            Point index = GetCellIndex(position);
            Vector2 cellCenter = GetCellCenter(index);

            //Choose which neighbouring cells to use for interpolation
            int offsetX, offsetY;
            if (index.x == 0) offsetX = 1;
            else if (index.x == CountX - 1) offsetX = -1;
            else if (position.x > cellCenter.x) offsetX = 1;
            else offsetX = -1;

            if (index.y == 0) offsetY = 1;
            else if (index.y == CountY - 1) offsetY = -1;
            else if (position.y > cellCenter.y) offsetY = 1;
            else offsetY = -1;

            //Compute interpolation coefficients
            Vector2 x1y1Pos = GetCellCenter(index.x, index.y);
            Vector2 x2y1Pos = GetCellCenter(index.x + offsetX, index.y);
            Vector2 x1y2Pos = GetCellCenter(index.x, index.y + offsetY);

            float kx = Math.MapFromClamped(x1y1Pos.x, x2y1Pos.x, position.x);
            float ky = Math.MapFromClamped(x1y1Pos.y, x1y2Pos.y, position.y);

            //Compute the bilinear interpolation
            T x1y1Value = this[index.x, index.y];
            T x2y1Value = this[index.x + offsetX, index.y];
            T x1y2Value = this[index.x, index.y + offsetY];
            T x2y2Value = this[index.x + offsetX, index.y + offsetY];

            T lerpy1 = interp(x1y1Value, x2y1Value, kx);
            T lerpy2 = interp(x1y2Value, x2y2Value, kx);
            T result = interp(lerpy1, lerpy2, ky);

            return result;
        }

        public (float kx, float ky, Point x1y1, Point x2y1, Point x1y2, Point x2y2) GetBillinearInterpolants(Vector2 position)
        {
            Point idx = GetCellIndex(position);
            Vector2 cellCenter = GetCellCenter(idx);

            Vector2 relPos = position - Min;

            int nx = (int)((relPos.x - CellSize.x * 0.5f) / CellSize.x);
            int ny = (int)((relPos.y - CellSize.y * 0.5f) / CellSize.y);

            float x1 = nx * CellSize.x + CellSize.x * 0.5f;
            float y1 = ny * CellSize.y + CellSize.y * 0.5f;

            float ex = Mathf.Clamp01((relPos.x - x1) / CellSize.x);
            float ey = Mathf.Clamp01((relPos.y - y1) / CellSize.y);

            if (nx > CountX - 2) nx = CountX - 2;
            if (ny > CountY - 2) ny = CountY - 2;
            return (ex, ey,
                new Point(nx, ny),
                new Point(nx + 1, ny),
                new Point(nx, ny + 1),
                new Point(nx + 1, ny + 1));
        }

        public bool Contains(Vector2 position)
        {
            return position.x >= m_min.x && position.x <= m_max.x &&
                position.y >= m_min.y && position.y <= m_max.y;
        }

        public bool Contains(int i, int j)
        {
            return i >= 0 && i < m_countX && j >= 0 && j < m_countY;
        }

        public bool Contains(Point p)
        {
            return Contains(p.x, p.y);
        }

        public static NativeSpatialGrid<T> CreateCopy<TSource>(NativeSpatialGrid<TSource> source) where TSource : struct
        {
            return new NativeSpatialGrid<T>(source.Min, source.Max, source.CountX, source.CountY);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }
    }
}