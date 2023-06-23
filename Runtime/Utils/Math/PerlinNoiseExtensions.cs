using UnityEngine;

namespace LBF.Math
{
    public struct MultiPerlinParameters
    {
        public float Scale;
        public float Persistence;
        public int Octaves;

        public static MultiPerlinParameters Default = new MultiPerlinParameters() { Scale = 250, Persistence = 0.75f, Octaves = 3 };
    }

    public static class PerlinNoiseExtensions
    {
        public static float Compute(this PerlinNoise perlinNoise, float x, float y, ref MultiPerlinParameters parameters)
        {
            return perlinNoise.Compute(x, y, parameters.Scale, parameters.Persistence, parameters.Octaves);
        }

        public static float Compute(this PerlinNoise perlinNoise, float x, float y, float scale, float persistence, int octaves)
        {
            float acc = 0.0f;
            for (int i = 0; i < octaves; i++)
                acc += perlinNoise.Noise(Mathf.Pow(2, i) * x / scale, Mathf.Pow(2, i) * y / scale) * Mathf.Pow(persistence, i);
            return acc;
        }
    }
}