using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class MyMath
    {
        public static int Round(double value)
        {
            int i = (int)value;

            int d = (int)(value * 10); //shift the decimal over one to the right
            d = d % 10; //extract the d

            if (d >= 5)
                return i + 1;

            return i;
        }

        /// <summary>
        /// Performs a CORRECT mod that handles negative numbers
        /// </summary>
        /// <param name="x"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static int Mod(int x, int mod)
        {
            return (x % mod + mod) % mod;
        }
    }
}
