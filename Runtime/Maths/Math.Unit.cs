namespace LBF
{
    public static partial class Math
    {
        public static float Invert(float u)
        {
            return 1 - u;
        }

        public static float Power(float u, float exponent)
        {
            return (float)System.Math.Pow(u, exponent);
        }

        public static float Smooth(float u)
        {
            return Math.Lerp(
                u * u * u,
                1 - (1 - u) * (1 - u) * (1 - u),
                u);
        }

        public static float Clamp01(float u)
        {
            if (u > 1) return 1;
            if (u < 0) return 0;
            return u;
        }

        public static float Loop(float t)
        {
            return t % 1.0f;
        }

        public static float PingPong(float t)
        {
            float u = t % 1.0f;
            int n = ((int)t);
            if (n % 2 == 0) return u;
            else return 1 - u;
        }

        public static float Lerp(float a, float b, float u)
        {
            return a * (1 - u) + b * u;
        }

        public static float Map(float inMin, float inMax, float outMin, float outMax, float t)
        {
            if (inMin == inMax) return (outMax + outMin) * 0.5f;
            return outMin + (outMax - outMin) * (t - inMin) / (inMax - inMin);
        }

        public static float MapClamped(float inMin, float inMax, float outMin, float outMax, float t)
        {
            if (t < inMin) return outMin;
            if (t > inMax) return outMax;
            float map = Map(inMin, inMax, outMin, outMax, t);
            return map;
        }

        public static float MapFrom(float inMin, float inMax, float t)
        {
            return Map(inMin, inMax, 0, 1, t);
        }

        public static float MapFromClamped(float inMin, float inMax, float t)
        {
            return Math.Clamp01(Map(inMin, inMax, 0, 1, t));
        }

        public static float MapTo(float outMin, float outMax, float u)
        {
            return Map(0, 1, outMin, outMax, u);
        }

        public static float Window(float start, float end, float u)
        {
            if (u >= start && u <= end) return 1;
            return 0;
        }

        public static float LinearStep(float start, float end, float u)
        {
            if (u < start) return 0;
            if (u > end) return 1;
            return (u - start) / (end - start);
        }

        public static float SmoothStep(float start, float end, float u)
        {
            return Smooth(LinearStep(start, end, u));
        }

        public static float Cos(float u)
        {
            double rad = u * 2 * System.Math.PI;
            return 0.5f + 0.5f * (float)System.Math.Cos(rad);
        }

        public static float Sin(float u)
        {
            double rad = u * 2 * System.Math.PI;
            return 0.5f + 0.5f * (float)System.Math.Sin(rad);
        }

        public static float Wave(float period, float t)
        {
            double rad = t / period * 2 * System.Math.PI;
            return 0.5f + 0.5f * (float)System.Math.Sin(rad);
        }

        public static float Triangle(float u)
        {
            u = Loop(u);
            if (u < 0.5f) return 2 * u;
            return 2 * (1 - u);
        }

        public static float Triangle(float u, float uPeak)
        {
            u = Loop(u);
            if (u < uPeak) return u / uPeak;
            else return 1 - (u - uPeak) / (1 - uPeak);
        }

        public static float Triangle(float inMin, float inPeak, float inMax, float t)
        {
            if (t < inPeak) return 1 - (inPeak - t) / (inPeak - inMin);
            else return 1 - (t - inPeak) / (inMax - inPeak);
        }

        public static float Bell(float u)
        {
            return Smooth(Triangle(u));
        }

        public static float BellFallout(float deviaton, float value)
        {
            float k = value / deviaton;
            return (float)System.Math.Pow(0.5f, k * k);
        }

        public static float ExponentialTransition(float from, float to, float halfLife, float deltaTime)
        {
            float k = (float)System.Math.Pow(0.5f, deltaTime / halfLife);
            return to + (from - to) * k;
        }

        public static float Progress(float duration, float deltaTime, float u)
        {
            return u + deltaTime / duration;
        }

        public static float ProgressClamped(float duration, float deltaTime, float u)
        {
            return Clamp01(u + deltaTime / duration);
        }

        public static float FadeOutInside(float start, float end, float fadeDistance, float t)
        {
            float dist = System.Math.Min(t - start, end - t);
            return SmoothStep(0, fadeDistance, dist);
        }

        public static float FadeOutOutside(float start, float end, float fadeDistance, float t)
        {
            bool inside = t > start && t < end;
            if (inside) return 1;
            float dist = System.Math.Max(start - t, t - end);
            return 1 - SmoothStep(0, fadeDistance, dist);
        }

        public static float FadeOutFrom(float origin, float fadeDistance, float t)
        {
            return 1 - SmoothStep(0, fadeDistance, System.Math.Abs(origin - t));
        }
    }
}
