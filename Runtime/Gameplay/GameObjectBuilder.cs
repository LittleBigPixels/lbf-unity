using System;
using UnityEngine;

namespace LBF
{
	public class GameObjectBuilder
	{
		GameObject m_gameObject;
		public GameObject GameObject => m_gameObject;

		public GameObjectBuilder( String name ) {
			m_gameObject = new GameObject( name );
		}

		public GameObjectBuilder( String name, GameObject parent ) {
			m_gameObject = new GameObject( name );
			m_gameObject.transform.SetParent( parent.transform, false );
		}

		public T RequireComponent<T>() where T : Component {
			return RequireComponent<T>( null );
		}

		public T RequireComponent<T>( Action<T> initializer ) where T : Component {
			var cmp = m_gameObject.GetComponent<T>();
			if (cmp == null) {
				cmp = AddComponent<T>();
				if (initializer != null) initializer( cmp );
			}

			return cmp;
		}

		public T AddComponent<T>() where T : Component {
			return AddComponent<T>( null );
		}

		public T AddComponent<T>( Action<T> initializer ) where T : Component {
			var cmp = m_gameObject.AddComponent<T>();
			if (initializer != null) initializer( cmp );
			return cmp;
		}

		public MeshRenderer AddMeshRenderer( Mesh mesh, Material material ) {
			var renderer = AddComponent<MeshRenderer>();
			renderer.sharedMaterial = material;

			var filter = RequireComponent<MeshFilter>();
			filter.sharedMesh = mesh;

			return renderer;
		}
		public static T CreateWithComponent<T>( string  name ) where T : Component {
			var gameObject = new GameObject( name );
			var component = gameObject.AddComponent<T>();
			return component;
		}
	}
}