using System.Drawing;
using Color = UberStrok.Core.Common.Color;
namespace UberStrok.Core.Common
{
    public struct Color
    {
        public Color(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
            // 255 aka 100% opacity
            A = 1;
        }

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color Convert(string hex)
        {
            var baseColor = ColorTranslator.FromHtml(hex);
            return new Color(baseColor.R / 255f, baseColor.G / 255f, baseColor.B / 255f, 255f / 255f);
        }

        public float R;
        public float G;
        public float B;
        public float A;

        public override bool Equals(object obj)
        {
            return obj is Color && this == (Color)obj;
        }

        public static bool operator ==(Color a, Color b)
        {
            return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        }

        public static bool operator !=(Color a, Color b)
        {
            return !(a == b);
        }
    }
}
