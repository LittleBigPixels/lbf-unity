using Sirenix.OdinInspector;
using UnityEngine;

namespace LBF
{
	public class CameraControllerComponent : SerializedMonoBehaviour
	{
		public ICameraController Controller;
		public bool AutoUpdate;

		public void Start() {
			if(Controller != null)
				Controller.Camera = Camera.main;
		}

		public void Update() {
			if (Controller.Camera == null)
				Controller.Camera = Camera.main;
			if(AutoUpdate)
				Controller.Update( Time.deltaTime );
		}
	}
}