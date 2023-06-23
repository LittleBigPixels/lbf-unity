using System;
using System.Collections.Generic;

namespace LBF.Structures {
    public class FastPool<T> where T : new()
    {
        const int MinPoolSize = 128;

        readonly List<T> m_instances;
        int m_takenCount;

        readonly Action<T> m_activator;

        public FastPool()
        {
            m_instances = new List<T>(MinPoolSize);
            Expand();

            m_takenCount = 0;
        }

        public FastPool(Action<T> activator)
        {
            m_activator = activator;

            m_instances = new List<T>(MinPoolSize);
            Expand();

            m_takenCount = 0;
        }

        public T Take()
        {
            if (m_takenCount >= m_instances.Count)
                Expand();

            return m_instances[m_takenCount++];
        }

        public void ReleaseAll()
        {
            m_takenCount = 0;
        }

        private void Expand()
        {
            m_instances.Capacity = System.Math.Max(MinPoolSize, m_instances.Capacity * 2);
            while (m_instances.Count < m_instances.Capacity)
            {
                T instance = new T();
                if (m_activator != null) m_activator(instance);

                m_instances.Add(instance);
            }
        }
    }
}