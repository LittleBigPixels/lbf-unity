using System;

namespace LBF.Gameplay.Updatables {
	public class SingleTaskSystem<T> {
		public event Action<IUpdatableTask<T>> OnTaskComplete;
	
		public IUpdatableTask<T> Task { get; private set; }

		readonly T m_target;

		public SingleTaskSystem( T target ) {
			Task = null;
			m_target = target;
		}

		public void Update() {
			if (Task != null)
				Task.Update();

			if (Task != null && Task.Complete) {
				if (OnTaskComplete != null) OnTaskComplete( Task );
				EndCurrent();
			}
		}

		public void StartTask( IUpdatableTask<T> task ) {
			EndCurrent();
			if (task == null) return;

			Task = task;
			Task.Target = m_target;
			Task.Start();
		}

		public void EndCurrent() {
			if (Task != null)
				Task.End();

			Task = null;
		}
	}
}