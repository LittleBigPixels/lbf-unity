using System.Collections.Generic;
using UnityEngine;

namespace LBF.Helpers
{
    public static class TransformExtensions
    {
        public static GameObject FindRecursive(this Transform transform, string name)
        {
            var children = transform.GetChildrenRecursive();
            foreach (var child in children)
                if (child.name == name) return child;
            return null;
        }

        public static IEnumerable<GameObject> GetChildren(this Transform transform)
        {
            if (transform != null)
            {
                var nChild = transform.childCount;
                for (int i = 0; i < nChild; i++)
                    yield return transform.GetChild(i).gameObject;
            }
        }

        public static IEnumerable<GameObject> GetChildrenRecursive(this Transform transform)
        {
            var nChild = transform.childCount;
            for (int i = 0; i < nChild; i++)
            {
                var child = transform.GetChild(i);
                yield return child.gameObject;
                foreach (var c in child.GetChildrenRecursive())
                    yield return c.gameObject;
            }
        }

        public static IEnumerable<T> GetComponentsInSelfAndChildrenIncludeInactive<T>(this Transform transform)
        {
            var componentInSelf = transform.GetComponents<T>();
            foreach (var component in componentInSelf)
                yield return component;

            var nChild = transform.childCount;
            for (int i = 0; i < nChild; i++)
            {
                var child = transform.GetChild(i);
                foreach (var component in child.GetComponentsInSelfAndChildrenIncludeInactive<T>())
                    yield return component;
            }
        }

        public static IEnumerable<T> GetComponentsInSelfAndChildren<T>(this Transform transform)
        {
            var componentInSelf = transform.GetComponents<T>();
            foreach (var component in componentInSelf)
                yield return component;

            var nChild = transform.childCount;
            for (int i = 0; i < nChild; i++)
            {
                var child = transform.GetChild(i);
                if (child.gameObject.activeInHierarchy == false) continue;
                foreach (var component in child.GetComponentsInSelfAndChildren<T>())
                    yield return component;
            }
        }

        public static void GetComponentsInSelfAndChildren<T>(this Transform transform, List<T> results)
        {
            var componentInSelf = transform.GetComponent<T>();
            if (componentInSelf != null) results.Add(componentInSelf);
            //foreach (var component in componentInSelf)
            //    results.Add(component);

            var nChild = transform.childCount;
            for (int i = 0; i < nChild; i++)
            {
                var child = transform.GetChild(i);
                if (child.gameObject.activeInHierarchy == false) continue;
                child.GetComponentsInSelfAndChildren<T>(results);
            }
        }
    }
}