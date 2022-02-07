using UnityEngine;

namespace BaseClocks
{
    public static class ColorExtensions
    {
        public static Color SetRed(this Color c, float r)
        {
            return new Color(r, c.g, c.b);
        }

        public static Color SetGreen(this Color c, float g)
        {
            return new Color(c.r, g, c.b);
        }

        public static Color SetBlue(this Color c, float b)
        {
            return new Color(c.r, c.g, b);
        }

        public static Color SetAlpha(this Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }
    }
}
