using System;
using System.Collections.Generic;

namespace LBF.Helpers
{
    public static partial class RandomExtensions
    {
        /// <summary>
        /// Returns a random float between 0 and 1.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static float NextFloat01(this Random random)
        {
            return (float)random.NextDouble();
        }

        /// <summary>
        /// Return a random float between two values
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float NextFloat(this Random random, float min, float max)
        {
            return min + (max - min) * (float)random.NextDouble();
        }

        /// <summary>
        /// Returns a random float between 0 and 2 PI.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static float NextAngle(this Random random)
        {
            return MathF.PI * 2 * (float)random.NextDouble();
        }

        /// <summary>
        /// Returns -1 or +1 randomly.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static float NextSign(this Random random)
        {
            if (random.NextFloat01() >= 0.5f)
                return 1.0f;
            return -1.0f;
        }

        /// <summary>
        /// Returns true or false randomly.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static bool NextBool(this Random random)
        {
            if (random.NextFloat01() >= 0.5f)
                return true;
            return false;
        }

        /// <summary>
        /// Returns a random value in a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="random"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T NextItem<T>(this Random random, IList<T> list)
        {
            if (list.Count == 0) return default(T);
            return list[random.Next(0, list.Count)];
        }

        /// <summary>
        /// Returns a random value in a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="random"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static T NextItem<T>(this Random random, HashSet<T> set)
        {
            if (set.Count == 0) return default(T);
            var enumerable = set.GetEnumerator();
            int idx = random.Next(0, set.Count);
            enumerable.MoveNext();
            for (int i = 0; i < idx; i++)
                enumerable.MoveNext();
            return enumerable.Current;
        }
    }
}
