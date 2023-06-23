using System.Collections.Generic;

namespace LBF.Helpers
{
    public static class ListExtensions
    {
        public static void SwapRemove<T>(this List<T> list, T value)
        {
            int index = list.IndexOf(value);
            list[index] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
        }

        public static void SwapRemove<T>(this List<T> list, int index)
        {
            list[index] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
        }
    }
}
