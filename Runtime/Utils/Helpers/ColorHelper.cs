using UnityEngine;

namespace LBF.Helpers
{
    /// <summary>
    /// Provide some functions for color manipulation.
    /// Formulas taken from http://www.easyrgb.com/
    /// </summary>
    public static class ColorHelper
    {
        public static Color FromVector3(Vector3 v)
        {
            return new Color(v.x, v.y, v.x);
        }

        public static Vector3 RgbToHsl(Vector3 color)
        {
            float h = 0, s = 0, l = 0;

            float min = System.Math.Min(System.Math.Min(color.x, color.y), color.z);
            float max = System.Math.Max(System.Math.Max(color.x, color.y), color.z);
            float delta = max - min;

            l = (max + min) * 0.5f;

            if (delta == 0)
            {
                h = 0.0f;
                s = 0.0f;
            }
            else
            {
                if (l < 0.5)
                {
                    s = delta / (max + min);
                }
                else
                {
                    s = delta / (2.0f - max - min);
                }

                float deltaR = (((max - color.x) / 6.0f) + (delta / 2.0f)) / delta;
                float deltaG = (((max - color.y) / 6.0f) + (delta / 2.0f)) / delta;
                float deltaB = (((max - color.z) / 6.0f) + (delta / 2.0f)) / delta;

                if (color.x == max)
                {
                    h = deltaB - deltaG;
                }
                else if (color.y == max)
                {
                    h = 1.0f / 3.0f + deltaR - deltaB;
                }
                else if (color.z == max)
                {
                    h = 2.0f / 3.0f + deltaG - deltaR;
                }

                if (h < 0)
                {
                    h += 1;
                }
                if (h > 1)
                {
                    h -= 1;
                }
            }
            return new Vector3(h, s, l);
        }

        public static Vector3 HslToRgb(Vector3 color)
        {
            float r, g, b;
            if (color.y == 0)                       
            {
                r = color.z;                      
                g = color.z;
                b = color.z;
            }
            else
            {
                float q, p;

                if (color.z < 0.5)
                {
                    p = color.z * (1 + color.y);
                }
                else
                {
                    p = (color.z + color.y) - (color.y * color.z);
                }

                q = 2 * color.z - p;

                r = HueToRgb(q, p, color.x + (1.0f / 3.0f));
                g = HueToRgb(q, p, color.x);
                b = HueToRgb(q, p, color.x - (1.0f / 3.0f));
            }
            return new Vector3(r, g, b);
        }

        public static float HueToRgb(float q, float p, float h)
        {
            if (h < 0) h += 1;
            if (h > 1) h -= 1;

            if ((6.0f * h) < 1) return (q + (p - q) * 6.0f * h);
            if ((2.0f * h) < 1) return (p);
            if ((3.0f * h) < 2) return (q + (p - q) * ((2.0f / 3.0f) - h) * 6.0f);
            
            return (q);
        }
    }
}
