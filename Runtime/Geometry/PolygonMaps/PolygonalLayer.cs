using System;
using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    [Serializable]
    public class PolygonalLayer<T>
    {
        [SerializeField]
        public T[] Values;

        public T this[int i] {
            get { return Values[i]; }
            set { Values[i] = value; }
        }

        public T this[Cell p] {
            get { return Values[p.Index]; }
            set { Values[p.Index] = value; }
        }

        public int PolygonCount { get { return Values.Length; } }

        public PolygonalLayer()
        {
            Values = new T[0];
        }

        public PolygonalLayer(int polygonCount)
        {
            Values = new T[polygonCount];
        }

        public PolygonalLayer(PolygonMap graph)
        {
            Values = new T[graph.PolygonCount];
        }

        public void ResetToValue(T value)
        {
            for (int i = 0; i < Values.Length; i++)
                Values[i] = value;
        }
    }
}
