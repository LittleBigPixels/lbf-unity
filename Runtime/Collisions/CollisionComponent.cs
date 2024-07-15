using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LBF;
using LBF.Geometry.Polygons;
using LBF.Geometry.Shapes;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;

[ExecuteInEditMode]
public class CollisionComponent : SerializedMonoBehaviour
{
    [TitleGroup("Shape")]
    public IShape Shape;

    Vector2[] m_shapePoints;
    Polygon m_transformedPolygon;

    public void OnValidate()
    {
        m_shapePoints = new Vector2[0];
        if (Shape != null) m_shapePoints = Shape.GetPoints();

        m_transformedPolygon = new Polygon(m_shapePoints);
    }

    public void OnDrawGizmosSelected()
    {
    }

    public void SetShape(IShape shape)
    {
        Shape = shape;
        m_shapePoints = shape.GetPoints();
        m_transformedPolygon = new Polygon(m_shapePoints);
    }

    public Vector2[] GetTransformedPoints()
    {
        if (m_shapePoints == null) OnValidate();
        for (int i = 0; i < m_transformedPolygon.Points.Length; i++)
            m_transformedPolygon.Points[i] = transform.TransformPoint(m_shapePoints[i].FromXZ()).XZ();

        m_transformedPolygon.SetPointsDirect(m_transformedPolygon.Points);

        return m_transformedPolygon.Points;
    }

    public Polygon GetTransformedPolygon()
    {
        if (m_shapePoints == null) OnValidate();
        for (int i = 0; i < m_transformedPolygon.Points.Length; i++)
            m_transformedPolygon.Points[i] = transform.TransformPoint(m_shapePoints[i].FromXZ()).XZ();

        m_transformedPolygon.SetPointsDirect(m_transformedPolygon.Points);

        return m_transformedPolygon;
    }
}