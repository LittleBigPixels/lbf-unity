using System;
using System.Collections.Generic;
using System.Linq;
using LBF;
using LBF.Geometry.Shapes;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollisionComponent))]
public class CollisionShapeEditor : OdinEditor
{
    const float ControlRadius = 0.3f;

    public override bool RequiresConstantRepaint() => true;

    private int m_hotControlIndex = -1;

    private void OnSceneGUI()
    {
        var collisionCmp = target as CollisionComponent;
        if (collisionCmp.Shape == null) return;
        if (collisionCmp.Shape is PolygonShape == false) return;

        var polygonShape = collisionCmp.Shape as PolygonShape;

        var evt = Event.current;

        List<Vector3> points = collisionCmp.GetTransformedPoints()
            .Select(p => p.FromXZ(collisionCmp.transform.position.y)).ToList();
        List<int> controlsID = points.Select(p => GUIUtility.GetControlID(FocusType.Passive)).ToList();

        for (int i = 0; i < points.Count; i++)
        {
            int controlID = controlsID[i];
            switch (evt.GetTypeForControl(controlID))
            {
                case EventType.Layout:
                case EventType.MouseMove:
                    var distanceToHandle = HandleUtility.DistanceToCircle(points[i], 1);
                    HandleUtility.AddControl(controlID, distanceToHandle);
                    break;
                
                case EventType.MouseDown:
                    if (controlID == HandleUtility.nearestControl && evt.button == 0)
                    {
                        Debug.Log("Set Hot Control to: " + controlID);
                        GUIUtility.hotControl = controlID;
                        m_hotControlIndex = i;
                        evt.Use();
                    }
                    break;
                
                case EventType.MouseDrag:
                    Debug.Log("MouseDrag hit control is: " + GUIUtility.hotControl);
                    if (m_hotControlIndex == i)
                    {
                        var worldRay = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                        var groundPosition = worldRay.origin +
                                             worldRay.direction * worldRay.origin.y / Mathf.Abs(worldRay.direction.y);
                        polygonShape.Polygon.Points[i] = (groundPosition - collisionCmp.transform.position).XZ();
                        collisionCmp.SetShape(polygonShape);
                        EditorUtility.SetDirty(collisionCmp);
                        evt.Use();
                        Repaint();
                    }
                    break;
                
                case EventType.MouseUp:
                    Debug.Log("Event MouseUp");
                    if (GUIUtility.hotControl == controlID && evt.button == 0)
                    {
                        Debug.Log("Unset Hot Control");
                        GUIUtility.hotControl = 0;
                        m_hotControlIndex = -1;
                        evt.Use();
                    }
                    break;
                
                case EventType.Repaint:
                    Debug.Log("Event Repaint");
                    Handles.color = new Color(1, 0.8f, 0.4f, 1);
                    if (controlID == HandleUtility.nearestControl)
                        Handles.color = new Color(1, 0.8f, 0.8f, 1);
                    Handles.DrawSolidDisc(points[i], Vector3.up, ControlRadius);
                    Handles.Label(points[i], $"{i}: {controlID}");
                    break;
            }
        }

        Handles.color = Color.red;
        for (int i = 0; i < points.Count; i++)
            Handles.DrawLine(points[i], points[(i + 1) % points.Count]);
    }
}