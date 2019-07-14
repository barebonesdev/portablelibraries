using System;
using System.Collections.Generic;
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
    }
}
