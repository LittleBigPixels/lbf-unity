using System;
using System.Collections.Generic;
using LBF;
using UnityEngine;

public class InteractionToolSystem
{
    public IInteractionTool DefaultTool { get; set; }
    public IInteractionTool CurrentTool { get; private set; }

    public InteractionToolSystem() { }

    private InteractionToolContext m_context;

    public void Start()
    {
        m_context = new InteractionToolContext();
        if (DefaultTool != null)
            SetTool(DefaultTool);
    }

    public void Update()
    {
        if (CurrentTool == null) return;

        m_context.CursorScreenPosition = Input.mousePosition;
        m_context.IsCursorInsideScreen = true;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        m_context.Position3D = ray.origin + ray.direction * ray.origin.y / Mathf.Abs(ray.direction.y);
        m_context.Position2D = m_context.Position3D.XZ();
        
        if (Input.GetMouseButtonDown(0))
            CurrentTool.OnConfirm(m_context);
        if (Input.GetMouseButtonDown(1))
            CurrentTool.OnCancel(m_context);

        if (!CurrentTool.Complete)
            CurrentTool.Update(m_context);
        
        if(CurrentTool.Complete)
            SetTool(DefaultTool);
    }

    public void End()
    {
        SetTool(null);
    }

    private void SetTool(IInteractionTool tool)
    {
        if (CurrentTool != null)
            CurrentTool.End();

        CurrentTool = tool;
        if (CurrentTool != null)
            CurrentTool.Start();
    }
}