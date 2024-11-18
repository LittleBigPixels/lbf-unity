using System;
using LBF;
using LBF.Gameplay.Camera;
using Sirenix.OdinInspector;
using UnityEngine;
using Math = System.Math;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class OrbitalCameraParameters
{
    [TitleGroup("Linear Speed")] public float LinearSpeed = 1.8f;
    public float LinearInertia = 0.0001f;

    [TitleGroup("Angular Speed")] public float AngleSpeed = 6.0f;
    public float AngleInertia = 0.00015f;

    [TitleGroup("Zoom Speed")] public float ZoomSpeed = 1.0f;
    public float ZoomInertia = 0.0001f;

    [TitleGroup("Terrain Feedback")] public float SmoothTerrainSpeed = 0.05f;
    public float PseudoTargetSize = 4.0f;
    public float PseudoTargetMinSize = 10.0f;
    public float PseudoSize = 4.0f;

    [TitleGroup("Zoom limits")] public int ZoomLevelMax = 25;
    public float ZoomDistanceMin = 4;
    public float ZoomDistanceMax = 80;

    [TitleGroup("Angle limits")] public float AngleVerticalMin = -5;
    public float AngleVerticalMax = 89;

    [TitleGroup("Controls")] public float MouseMoveSpeed = .1f;
}

[Serializable]
public class OrbitalCameraController : ICameraController
{
    //Events
    public event Action OnMove;
    public event Action OnRotate;
    public event Action OnZoom;

    //Parameters
    public OrbitalCameraParameters Parameters;

    //Properties
    public Camera Camera { get; set; }
    public float ZoomRatio => ZoomLevel / Parameters.ZoomLevelMax;
    public Vector3 Position => m_position;

    //State
    [TitleGroup("State")] public Vector3 Target;

    [TitleGroup("State - Position")] public float AngleHorizontal;
    public float AngleVertical;
    public float ZoomLevel;

    [TitleGroup("State - Velocities")] public Vector2 LinearVelocity;
    public Vector2 AngularVelocity;
    public float ZoomVelocity;

    //Computed values
    Vector3 m_position;
    Vector3 m_pseudoTarget;
    Vector3 m_pseudoPosition;

    Vector2 m_mousePosition = Vector2.zero;

    public OrbitalCameraController()
    {
        // Create default values
        var baseTargetPosition = Vector2.zero;
        Target = Vector3.zero;
        AngleHorizontal = 1.42f;
        AngleVertical = 0.2f;
        ZoomLevel = 11;

        // Create default parameters
        Parameters = new OrbitalCameraParameters();

        m_mousePosition = Input.mousePosition;
    }

    public bool MouseScreenCheck()
    {
#if UNITY_EDITOR
        if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 ||
            Input.mousePosition.x > Handles.GetMainGameViewSize().x - 1 ||
            Input.mousePosition.y > Handles.GetMainGameViewSize().y - 1)
        {
            return false;
        }
#else
        if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 || Input.mousePosition.x > UnityEngine.Screen.width - 1 || Input.mousePosition.y > UnityEngine.Screen.height - 1) {
            return false;
        }
#endif
        else
        {
            return true;
        }
    }

    public void Update(float dt)
    {
        float maxDeltaTime = 0.016f;
        float clampedDeltaTime = dt;
        if (clampedDeltaTime > maxDeltaTime)
            clampedDeltaTime = maxDeltaTime +
                               maxDeltaTime * (1 - Mathf.Exp(-(clampedDeltaTime - maxDeltaTime) / maxDeltaTime));

        var isMouseInScreen = MouseScreenCheck();

        //Inputs
        float zoomChange = 0f;
        Vector2 goalAngularSpeed = Vector2.zero;
        Vector2 targetLinearSpeed = Vector2.zero;

        if (isMouseInScreen)
        {
            //Inputs - Mouse
            if (Input.mouseScrollDelta.y > 0) zoomChange = 1.0f;
            if (Input.mouseScrollDelta.y < 0) zoomChange = -1.0f;

            if (Input.GetMouseButton(2))
            {
                var mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                goalAngularSpeed.x = -mouseDelta.x * Parameters.AngleSpeed;
                goalAngularSpeed.y = -mouseDelta.y * Parameters.AngleSpeed;
            }

            Vector2 mouseViewPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            if (Input.GetMouseButton(1))
            {
                mouseViewPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - m_mousePosition;
                targetLinearSpeed.x += mouseViewPosition.x * Parameters.MouseMoveSpeed;
                targetLinearSpeed.y += -1f * mouseViewPosition.y * Parameters.MouseMoveSpeed;
            }

            m_mousePosition = Input.mousePosition;

            //Inputs - Keyboard
            if (Input.GetKey(KeyCode.LeftArrow))
                targetLinearSpeed.x = 1;
            if (Input.GetKey(KeyCode.RightArrow))
                targetLinearSpeed.x = -1;
            if (Input.GetKey(KeyCode.UpArrow))
                targetLinearSpeed.y = 1;
            if (Input.GetKey(KeyCode.DownArrow))
                targetLinearSpeed.y = -1;
        }

        //Update zoom
        if (zoomChange != 0)
            if (OnZoom != null)
                OnZoom();
        
        ZoomVelocity = Damp(ZoomVelocity, zoomChange * Parameters.ZoomSpeed, Parameters.ZoomInertia, clampedDeltaTime);
        ZoomLevel = Mathf.Clamp(ZoomLevel - ZoomVelocity, 0, Parameters.ZoomLevelMax);
        var zoomDistance = DistanceFromZoomLevel(ZoomLevel);

        //Update angles
        if (goalAngularSpeed != Vector2.zero)
            if (OnRotate != null)
                OnRotate();

        AngularVelocity.x = Damp(AngularVelocity.x, goalAngularSpeed.x, Parameters.AngleInertia, clampedDeltaTime);
        AngularVelocity.y = Damp(AngularVelocity.y, goalAngularSpeed.y, Parameters.AngleInertia, clampedDeltaTime);

        AngleHorizontal += AngularVelocity.x * clampedDeltaTime;
        AngleVertical += AngularVelocity.y * clampedDeltaTime;

        AngleVertical = Mathf.Clamp(AngleVertical, Parameters.AngleVerticalMin * Mathf.Deg2Rad,
            Parameters.AngleVerticalMax * Mathf.Deg2Rad);

        //Update target position
        if (targetLinearSpeed != Vector2.zero)
            if (OnMove != null)
                OnMove();

        LinearVelocity.x = Damp(LinearVelocity.x, targetLinearSpeed.x, Parameters.LinearInertia, clampedDeltaTime);
        LinearVelocity.y = Damp(LinearVelocity.y, targetLinearSpeed.y, Parameters.LinearInertia, clampedDeltaTime);

        //Update orbital position
        Vector3 orbitalOffset = zoomDistance * new Vector3(
            (float)Math.Cos(AngleHorizontal) * (float)Math.Cos(AngleVertical),
            (float)Math.Sin(AngleVertical),
            (float)Math.Sin(AngleHorizontal) * (float)Math.Cos(AngleVertical));

        Vector3 camForward = -orbitalOffset;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = Vector3.Cross(camForward, Vector3.up);
        float camSpeed = zoomDistance * Parameters.LinearSpeed;
        Target += camSpeed * LinearVelocity.x * camRight * clampedDeltaTime;
        Target += camSpeed * LinearVelocity.y * camForward * clampedDeltaTime;

        m_pseudoTarget.x = Target.x;
        m_pseudoTarget.z = Target.z;

        float pseudoTargetSizeRatio =
            Mathf.Max(Parameters.PseudoTargetSize * ZoomLevel, Parameters.PseudoTargetMinSize);

        if (m_pseudoTarget.y - Target.y >= pseudoTargetSizeRatio)
            m_pseudoTarget.y = Target.y + pseudoTargetSizeRatio;
        else if (Target.y - m_pseudoTarget.y >= pseudoTargetSizeRatio)
            m_pseudoTarget.y = Target.y - pseudoTargetSizeRatio;
        else
            m_pseudoTarget.y = Mathf.Lerp(m_pseudoTarget.y, Target.y, Parameters.SmoothTerrainSpeed);

        //Check target visibility from target
        m_position = m_pseudoTarget + orbitalOffset;

        m_pseudoPosition = m_position;
        var cameraTerrainPos = m_pseudoPosition;
        if (m_pseudoPosition.y - cameraTerrainPos.y < Parameters.PseudoSize)
            m_pseudoPosition.y = cameraTerrainPos.y + Parameters.PseudoSize;

        Quaternion rotation = Quaternion.LookRotation(m_pseudoTarget - m_pseudoPosition, Vector3.up);

        //Update camera
        Camera.transform.position = m_pseudoPosition;
        Camera.transform.rotation = rotation;
    }

    public static float Damp(float source, float target, float smoothing, float dt)
    {
        return Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
    }

    public void SetPosition(Vector3 target, Vector3 position)
    {
        Target = target;

        var distance = Vector3.Distance(position, target);
        ZoomLevel = ZoomLevelFromDistance(distance);

        LinearVelocity = Vector2.zero;
        AngularVelocity = Vector2.zero;
        ZoomVelocity = 0;

        var dir = Vector3.Normalize(position - target);
        var dir2D = dir.XZ();
        AngleHorizontal = -Vector2.SignedAngle(dir2D.normalized, new Vector2(1, 0)) * Mathf.Deg2Rad;
        AngleVertical = Mathf.Asin(dir.y);
    }

    public void OnEnabled() { }
    public void OnDisable() { }

    public float DistanceFromZoomLevel(float zoomLevel)
    {
        var zoomRatio = (float)zoomLevel / Parameters.ZoomLevelMax;
        var lerpFactor = zoomRatio * zoomRatio;
        return Mathf.Lerp(Parameters.ZoomDistanceMin, Parameters.ZoomDistanceMax, lerpFactor);
    }

    public float ZoomLevelFromDistance(float distance)
    {
        var lerpFactor = LBF.Math.MapFromClamped(Parameters.ZoomDistanceMin, Parameters.ZoomDistanceMax, distance);
        var zoomRatio = Mathf.Sqrt(lerpFactor);
        return zoomRatio * Parameters.ZoomLevelMax;
    }

    public void DisplayDebug()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Target, 1.5f);

        Gizmos.DrawLine(
            m_pseudoTarget + Vector3.up *
            Mathf.Max(Parameters.PseudoTargetSize * ZoomLevel, Parameters.PseudoTargetMinSize),
            m_pseudoTarget + Vector3.down *
            Mathf.Max(Parameters.PseudoTargetSize * ZoomLevel, Parameters.PseudoTargetMinSize));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(m_pseudoTarget, Camera.transform.position);

        Gizmos.color = Color.blue;

        Gizmos.DrawSphere(m_position, 1.5f);
        Gizmos.DrawLine(m_pseudoPosition, m_pseudoPosition + Vector3.down * Parameters.PseudoSize);

        Gizmos.matrix = Matrix4x4.TRS(Camera.transform.position, Camera.transform.rotation, Vector3.one);
        Gizmos.DrawFrustum(Camera.transform.position, Camera.fieldOfView, 10.0f, 0.1f, Camera.aspect);
    }

    public void ChangePosition(Vector3 pos)
    {
        SetPosition(Target, pos);
    }
}