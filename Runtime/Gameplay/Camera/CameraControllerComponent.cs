using Sirenix.OdinInspector;
using UnityEngine;

namespace LBF.Gameplay.Camera
{
	public class CameraControllerComponent : SerializedMonoBehaviour
	{
		public ICameraController Controller;
		public bool AutoUpdate;

		public void Start() {
			if(Controller != null)
				Controller.Camera = UnityEngine.Camera.main;
		}

		public void Update() {
			if (Controller.Camera == null)
				Controller.Camera = UnityEngine.Camera.main;
			if(AutoUpdate)
				Controller.Update( Time.deltaTime );
		}
	}
}