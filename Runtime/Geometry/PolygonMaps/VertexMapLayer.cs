using System;
using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    [Serializable]
    public class VertexMapLayer<T>
    {
        [SerializeField]
        public T[] Values;

        public T this[int i] {
            get { return Values[i]; }
            set { Values[i] = value; }
        }

        public T this[Vertex p] {
            get { return Values[p.Index]; }
            set { Values[p.Index] = value; }
        }

        public int VertexCount { get { return Values.Length; } }

        public VertexMapLayer()
        {
            Values = new T[0];
        }

        public VertexMapLayer(int vertexCount)
        {
            Values = new T[vertexCount];
        }

        public VertexMapLayer(PolygonMap map)
        {
            Values = new T[map.Vertices.Length];
        }

        public void ResetToValue(T value)
        {
            for (int i = 0; i < Values.Length; i++)
                Values[i] = value;
        }  
        
        public void CopyTo<T>(VertexMapLayer<T> target)
        {
            Values.CopyTo(target.Values, 0);
        }
    }
}
