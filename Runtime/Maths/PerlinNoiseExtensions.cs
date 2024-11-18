﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace LBF
{
	[Serializable]
    public struct MultiPerlinParameters
    {
        public float Scale;
        public float Persistence;
        public int Octaves;

        public static MultiPerlinParameters Default = new MultiPerlinParameters() { Scale = 250, Persistence = 0.75f, Octaves = 3 };

        public MultiPerlinParameters(float scale, float persistence, int octaves)
        {
	        Scale = scale;
	        Persistence = persistence;
	        Octaves = octaves;
        }
    }
	
    [Serializable]
	public class CustomPerlinParameters
	{
		[Serializable]
		public struct Octave
		{
			public float Scale;
			[Range(0f, 1f)] public float Strength;
		} 

		public List<Octave> Octaves = new();

		public static CustomPerlinParameters Default = new CustomPerlinParameters()
		{
			Octaves = new List<Octave>() {
				 new Octave() { Scale = 2000, Strength  =1 }, new Octave() { Scale = 1200, Strength = 0.8f} }
		};
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
		
		public static float Compute(this PerlinNoise perlinNoise, float x, float y, ref CustomPerlinParameters parameters)
		{
			var acc = 0.0f;
			for (int i = 0; i < parameters.Octaves.Count; i++)
				acc += perlinNoise.Noise(x / parameters.Octaves[i].Scale, y / parameters.Octaves[i].Scale) * parameters.Octaves[i].Strength;
			return acc;
		}
    }
}