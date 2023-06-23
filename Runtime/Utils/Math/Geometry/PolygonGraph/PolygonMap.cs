using System;
using System.Collections.Generic;

namespace LBF.Math.Geometry.PolygonGraph
{
    public class PolygonMap
    {
        PolygonalGraph m_polygons;
        public PolygonalGraph PolygonGraph {
            get { return m_polygons; }
        }

        Dictionary<String, Object> m_layers;
        public Dictionary<String, Object> Layers {
            get { return m_layers; }
        }

        float m_width;
        public float Width {
            get { return m_width; }
        }

        float m_height;
        public float Height {
            get { return m_height; }
        }

        public float Size {
            get { return System.Math.Max(m_height, m_width); }
        }

        public PolygonMap(PolygonalGraph polygonGraph, float width, float height)
        {
            m_polygons = polygonGraph;
            m_width = width;
            m_height = height;

            m_layers = new Dictionary<string, object>();
        }

        public void SetLayers(Dictionary<String, Object> layers)
        {
            m_layers = layers;
        }

        public PolygonalLayer<T> CreateOrGetLayer<T>(String name)
        {
            var layer = GetLayer<T>(name);
            if (layer != null) return layer;

            var newLayer = new PolygonalLayer<T>(m_polygons);
            m_layers[name] = newLayer;
            return newLayer;
        }

        public PolygonalLayer<T> CreateOrGetLayer<T>(Enum e)
        {
            return CreateOrGetLayer<T>(e.ToString());
        }

        public PolygonalLayer<T> GetLayer<T>(String name)
        {
            if(m_layers.ContainsKey(name) && m_layers[name] is PolygonalLayer<T>)
                return m_layers[name] as PolygonalLayer<T>;
            return null;
        }

        public PolygonalLayer<T> GetLayer<T>(Enum e)
        {
            return GetLayer<T>(e.ToString());
        }

        public void Iterate(Action<Polygon> action)
        {
            for (int i = 0; i < m_polygons.PolygonCount; i++)
                action(m_polygons.Polygons[i]);
        }
    }
}
