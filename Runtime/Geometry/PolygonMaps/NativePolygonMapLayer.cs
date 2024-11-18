using System;
using Unity.Collections;
using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    [Serializable]
    public struct NativePolygonMapLayer<T> where T : struct
    {
        public NativeArray<T> Values;

        public T this[int i]
        {
            get { return Values[i]; }
            set { Values[i] = value; }
        }

        public T this[Cell p]
        {
            get { return Values[p.Index]; }
            set { Values[p.Index] = value; }
        }

        public int PolygonCount
        {
            get { return Values.Length; }
        }

        public NativePolygonMapLayer(int polygonCount, Allocator allocator = Allocator.Persistent)
        {
            Values = new NativeArray<T>(polygonCount, allocator);
        }

        public NativePolygonMapLayer(PolygonMap map, Allocator allocator = Allocator.Persistent)
        {
            Values = new NativeArray<T>(map.PolygonCount, allocator);
        }

        public void ResetToValue(T value)
        {
            for (int i = 0; i < Values.Length; i++)
                Values[i] = value;
        }

        public void CopyTo(NativePolygonMapLayer<T> target)
        {
            Values.CopyTo(target.Values);
        }

        public void Dispose()
        {
            Values.Dispose();
        }
    }
}