using System;
using System.Collections.Generic;
using LBF.Gameplay.Updatables;
using UnityEngine;

public interface IInteractionTool
{
    bool Complete { get; }
    
    void Start();
    void Update(InteractionToolContext context);
    void OnConfirm(InteractionToolContext context);
    void OnCancel(InteractionToolContext context);
    void End();
}