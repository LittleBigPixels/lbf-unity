using System.Collections.Generic;
using UnityEngine;

namespace LBF.Structures
{
    public class PrefabPool<T> where T : Component
    {
        readonly Stack<T> m_instances;
        readonly HashSet<T> m_activeInstances;

        readonly GameObject m_parent;
        readonly T m_prefab;

        public PrefabPool(GameObject parent, T prefab)
        {
            m_instances = new Stack<T>();
            m_activeInstances = new HashSet<T>();

            m_parent = parent;
            m_prefab = prefab;
        }
        
        public T Activate()
        {
            if (m_instances.Count == 0)
            {
                T newInstance = GameObject.Instantiate<T>(m_prefab);
                newInstance.transform.SetParent(m_parent?.transform);
                newInstance.gameObject.SetActive(false);
                m_instances.Push(newInstance);
            }

            T instance = m_instances.Pop();
            instance.gameObject.SetActive(true);
            m_activeInstances.Add(instance);

            return instance;
        }

        public void Release(T instance)
        {
            m_activeInstances.Remove(instance);
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(m_parent?.transform);
            m_instances.Push(instance);
        }

        public void ReleaseAll()
        {
            foreach (T instance in m_activeInstances)
            {
                instance.gameObject.SetActive(false);
                instance.transform.SetParent(m_parent?.transform);
                m_instances.Push(instance);
            }
            m_activeInstances.Clear();
        }

        public void Clear()
        {
            foreach (T instance in m_instances)
                GameObject.Destroy(instance.gameObject);

            foreach (T instance in m_activeInstances)
                GameObject.Destroy(instance.gameObject);

            m_instances.Clear();
            m_activeInstances.Clear();
        }

        public void ClearKeepActive()
        {
            foreach (T instance in m_instances)
                GameObject.Destroy(instance.gameObject);
            m_instances.Clear();
            m_activeInstances.Clear();
        }
    }
}
