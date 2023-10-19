using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LBF.Gameplay.Camera
{
    public interface ICameraController
    {
        UnityEngine.Camera Camera { get; set; }
        void Update(float dt);
    }

    [Serializable]
    public class CameraController2D : ICameraController
    {
        public enum PlaneAxis
        {
            XY,
            XZ,
        }

        public UnityEngine.Camera Camera { get; set; }

        //Parameters
        [TitleGroup("Parameters - View")]
        public PlaneAxis Plane;
        [SuffixLabel("px")] public int ViewSize = 5000;

        [TitleGroup("Parameters - Zoom")]
        public float ZoomMultiplier = 1.5f;
        public int MaxZoomLevel = 10;

        [TitleGroup("Parameters - Movement")]
        public float MousePanSpeed = 1;
        public float KeyboardPanSpeed = 1;

        //State
        [TitleGroup("State")]
        public int ZoomLevel = 0;
        public Vector2 Offset;

        Vector2 m_dragBasePosition;
    
        public CameraController2D() {
            Camera = null;
        }
    
        public CameraController2D(UnityEngine.Camera target) {
            Camera = target;
        }

        public void Update(float dt)
        {
            if (Input.mouseScrollDelta != Vector2.zero)
                ZoomLevel += (int)Mathf.Sign(Input.mouseScrollDelta.y);
            ZoomLevel = Mathf.Clamp(ZoomLevel, 0, MaxZoomLevel);

            Camera.orthographicSize = ViewSize / Mathf.Pow(ZoomMultiplier, ZoomLevel);
            float speedCoef = Camera.orthographicSize / ViewSize;

            if (Input.GetMouseButtonDown( 0 )) {
                m_dragBasePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                Offset -= Input.GetAxis("Mouse X") * speedCoef * MousePanSpeed * Vector2.right;
                Offset -= Input.GetAxis("Mouse Y") * speedCoef * MousePanSpeed * Vector2.up;
            }

            if (Input.GetKey(KeyCode.UpArrow))
                Offset += speedCoef * KeyboardPanSpeed * dt * Vector2.up;
            if (Input.GetKey(KeyCode.DownArrow))
                Offset -= speedCoef * KeyboardPanSpeed * dt * Vector2.up;
            if (Input.GetKey(KeyCode.LeftArrow))
                Offset += speedCoef * KeyboardPanSpeed * dt * Vector2.left;
            if (Input.GetKey(KeyCode.RightArrow))
                Offset += speedCoef * KeyboardPanSpeed * dt * Vector2.right;

            if (Input.GetKey(KeyCode.W))
                Offset += speedCoef * KeyboardPanSpeed * dt * Vector2.up;
            if (Input.GetKey(KeyCode.A))
                Offset -= speedCoef * KeyboardPanSpeed * dt * Vector2.up;
            if (Input.GetKey(KeyCode.Q))
                Offset += speedCoef * KeyboardPanSpeed * dt * Vector2.left;
            if (Input.GetKey(KeyCode.D))
                Offset += speedCoef * KeyboardPanSpeed * dt * Vector2.right;

            if (Camera == null) return;
        
            if (Plane == PlaneAxis.XY)
                Camera.transform.position = new Vector3(Offset.x, Offset.y, -10);
            else if (Plane == PlaneAxis.XZ)
                Camera.transform.position = new Vector3(Offset.x, 10, Offset.y);
        }
    }
}