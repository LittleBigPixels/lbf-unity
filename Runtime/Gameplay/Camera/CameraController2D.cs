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

        public enum BoundMethod
        {
            None,
            Fill,
            Fit,
        }

        public UnityEngine.Camera Camera { get; set; }

        //Parameters
        [TitleGroup("Parameters - View")] public PlaneAxis Plane;
        [SuffixLabel("px")] public int ViewSize = 5000;
        public BoundMethod BoundType;
        [NonSerialized] public Bounds2D Bounds;

        [TitleGroup("Parameters - Zoom")] public float ZoomMultiplier = 1.5f;
        [Range(0, 100)] public int MaxZoomLevel = 10;

        [TitleGroup("Parameters - Movement")] public float MousePanSpeed = 1;
        public float KeyboardPanSpeed = 1;

        //State
        [TitleGroup("State")] public float ZoomLevel = 0;
        public Vector2 Offset;

        Vector2 m_dragBasePosition;

        public CameraController2D()
        {
            Camera = null;
        }

        public CameraController2D(UnityEngine.Camera target)
        {
            Camera = target;
        }

        public void Update(float dt)
        {
            var isMouseInScreen =
                Input.mousePosition.x >= 0 &&
                Input.mousePosition.y >= 0 &&
                Input.mousePosition.x <= Screen.width &&
                Input.mousePosition.y <= Screen.height;

            //Camera zoom
            if (isMouseInScreen && Input.mouseScrollDelta != Vector2.zero)
                ZoomLevel += (int)Mathf.Sign(Input.mouseScrollDelta.y);
            ZoomLevel = Mathf.Clamp(ZoomLevel, 0, MaxZoomLevel);

            float orthoSize = ViewSize / Mathf.Pow(ZoomMultiplier, ZoomLevel);
            float aspectRatio = Camera.aspect;
            float ratioMaxX = 2 * orthoSize * aspectRatio / Bounds.Size.x;
            float ratioMaxY = 2 * orthoSize / Bounds.Size.y;

            float ratio = 1;
            if (BoundType == BoundMethod.Fill)
                ratio = Mathf.Max(ratioMaxX, ratioMaxY);
            else
                ratio = Mathf.Min(ratioMaxX, ratioMaxY);
            if (ratio > 1)
            {
                orthoSize = orthoSize / ratio;
                ZoomLevel = -Mathf.Log(orthoSize / ViewSize, ZoomMultiplier);
            }

            Camera.orthographicSize = orthoSize;

            //Camera position
            float speedCoef = Camera.orthographicSize / ViewSize;

            if (Input.GetMouseButtonDown(0))
            {
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

            float maxOffsetX = Bounds.Size.x / 2 - Camera.orthographicSize * Camera.aspect;
            if (maxOffsetX < 0) maxOffsetX = 0;
            float maxOffsetY = Bounds.Size.y / 2 - Camera.orthographicSize;
            if (maxOffsetY < 0) maxOffsetY = 0;
            Offset.x = Mathf.Clamp(Offset.x, -maxOffsetX, maxOffsetX);
            Offset.y = Mathf.Clamp(Offset.y, -maxOffsetY, maxOffsetY);

            //Update camera transform
            if (Camera == null) return;

            if (Plane == PlaneAxis.XY)
                Camera.transform.position = new Vector3(Offset.x, Offset.y, -10);
            else if (Plane == PlaneAxis.XZ)
                Camera.transform.position = new Vector3(Offset.x, 10, Offset.y);
        }
    }
}