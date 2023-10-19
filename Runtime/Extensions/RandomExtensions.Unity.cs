using UnityEngine;

namespace LBF
{
    public static partial class RandomExtensions
    {
        /// <summary>
        /// Returns a random value with a power of 2 distribution over 0..1 range (more chance for higher values)
        /// This is useful for sampling a point in a circle for example
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static float NextFloatPow2(this System.Random random)
        {
            //We actually need to use the inverse function here
            return Mathf.Sqrt(random.NextFloat01());
        }

        /// <summary>
        /// Returns a random normalized 2d vector
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static Vector2 NextVector2(this System.Random random)
        {
            float angle = random.NextFloat01() * 2 * (float)System.Math.PI;
            return new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));
        }

        /// <summary>
        /// Returns a random normalized 2d vector
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static Vector3 NextVector3(this System.Random random)
        {
            float theta = random.NextFloat01() * 2 * (float)System.Math.PI;
            float phi = random.NextFloat01() * 2 * (float)System.Math.PI;
            return new Vector3(
                (float)System.Math.Cos(theta) * (float)System.Math.Sin(phi),
                (float)System.Math.Sin(theta) * (float)System.Math.Sin(phi),
                (float)System.Math.Cos(phi));
        }

        /// <summary>
        /// Return a random neutral color
        /// </summary>
        /// <returns></returns>
        public static Color NextColor(this System.Random random)
        {
            Color color = new Color(
                random.NextFloat(0.3f, 0.85f),
                random.NextFloat(0.3f, 0.85f),
                random.NextFloat(0.3f, 0.85f),
                1.0f);
            return color;
        }
    }
}
