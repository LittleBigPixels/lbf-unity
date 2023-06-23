using UnityEngine;

namespace LBF.Helpers
{
    /// <summary>
    /// Provide some functions for color manipulation.
    /// Formulas taken from http://www.easyrgb.com/
    /// </summary>
    public static class ColorExtensions {
        public static Vector3 ToVector3( this Color color ) {
            return new Vector3( color.r, color.g, color.b );
        }

        public static Color SetSaturation( this Color color, float saturation ) {
            Vector3 hsl = ColorHelper.RgbToHsl( color.ToVector3() );
            hsl.y = saturation;
            return ColorHelper.FromVector3( ColorHelper.HslToRgb( hsl ) );
        }
    }
}
