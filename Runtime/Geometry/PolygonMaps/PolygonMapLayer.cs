using System;
using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    [Serializable]
    public class PolygonMapLayer<T>
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

        public PolygonMapLayer()
        {
            Values = new T[0];
        }

        public PolygonMapLayer(int polygonCount)
        {
            Values = new T[polygonCount];
        }

        public PolygonMapLayer(PolygonMap map)
        {
            Values = new T[map.PolygonCount];
        }

        public void ResetToValue(T value)
        {
            for (int i = 0; i < Values.Length; i++)
                Values[i] = value;
        }  
        
        public void CopyTo<T>(PolygonMapLayer<T> target)
        {
            Values.CopyTo(target.Values, 0);
        }
    }
}
