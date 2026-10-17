using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public static class ColorHelper
    {
        public static byte[] ToBytes(string color)
        {
            color = color.TrimStart('#');

            return Enumerable.Range(0, color.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(color.Substring(x, 2), 16))
                     .ToArray();
        }

        public static string ToString(byte[] bytes)
        {
            return "#" + string.Join("", bytes.Select(b => ByteToString(b)));
        }

        private static string ByteToString(byte b)
        {
            string s = Convert.ToString(b, 16);
            if (s.Length == 1)
            {
                return "0" + s;
            }
            return s;
        }

        public static Color AdjustLightness(Color color, double amount)
        {
            RgbToHsl(color.R, color.G, color.B, out double h, out double s, out double l);
            l = Clamp(l + amount, 0.0, 1.0);
            HslToRgb(h, s, l, out int r, out int g, out int b);
            return Color.FromArgb(r, g, b);
        }

        internal static void RgbToHsl(int r, int g, int b, out double h, out double s, out double l)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));
            double diff = max - min;

            l = (max + min) / 2.0;

            if (Math.Abs(diff) < 0.00001)
            {
                h = 0;
                s = 0;
            }
            else
            {
                s = l > 0.5
                    ? diff / (2.0 - max - min)
                    : diff / (max + min);

                if (Math.Abs(max - rd) < 0.00001)
                    h = ((gd - bd) / diff) + (gd < bd ? 6 : 0);
                else if (Math.Abs(max - gd) < 0.00001)
                    h = ((bd - rd) / diff) + 2;
                else
                    h = ((rd - gd) / diff) + 4;

                h /= 6.0;
            }
        }

        internal static void HslToRgb(double h, double s, double l, out int r, out int g, out int b)
        {
            if (Math.Abs(s) < 0.00001)
            {
                r = g = b = (int)Math.Round(l * 255);
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = (int)Math.Round(HueToRgb(p, q, h + 1.0 / 3.0) * 255);
                g = (int)Math.Round(HueToRgb(p, q, h) * 255);
                b = (int)Math.Round(HueToRgb(p, q, h - 1.0 / 3.0) * 255);
            }

            r = Clamp(r, 0, 255);
            g = Clamp(g, 0, 255);
            b = Clamp(b, 0, 255);
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
