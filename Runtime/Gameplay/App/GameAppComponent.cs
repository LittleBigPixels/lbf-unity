using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameAppComponent : SerializedMonoBehaviour
{
    private IGameApp m_gameApp;
    private bool m_flagReset;

    public static GameAppComponent Create(IGameApp app)
    {
        var gameObject = new GameObject("Game");
        var gameAppCmp = gameObject.AddComponent<GameAppComponent>();
        gameAppCmp.StartApp(app);

        return gameAppCmp;
    }
    
    public void StartApp(IGameApp app)
    {
        m_gameApp = app;
        m_gameApp.Start();
    }

    public void ResetApp()
    {
        if (m_gameApp == null) return;
        m_flagReset = true;
    }

    private void Update()
    {
        if (m_gameApp == null) return;
        
        if (m_flagReset)
        {
            m_flagReset = false;
            m_gameApp.End();
            m_gameApp.Start();
        }
        
        m_gameApp.Update();
    }

    private void OnDestroy()
    {
        if (m_gameApp == null) return;
        m_gameApp.End();
    }
}