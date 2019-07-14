using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class Encryption
    {
        //All 20 in length
        private static byte[] secret1 = new byte[] { 69, 246, 85, 22, 156, 151, 214, 147, 111, 71, 180, 88, 251, 52, 253, 140, 103, 128, 21, 52 };
        private static byte[] secret2 = new byte[] { 182, 3, 38, 221, 209, 12, 150, 117, 103, 161, 61, 207, 61, 180, 175, 183, 251, 246, 95, 152 };

        public static byte[] Salt(string str)
        {
            byte[] bytes = new UTF8Encoding().GetBytes(str);
            Salt(bytes);
            return bytes;
        }

        public static string FakeDisplay(byte[] data)
        {
            StringBuilder answer = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                answer.Append('0');
            return answer.ToString();
        }

        /// <summary>
        /// Performs the salt on the incoming array (changes the incoming array)
        /// </summary>
        /// <param name="bytes"></param>
        public static void Salt(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] += secret1[i % 20];

                if (i % 2 == 0)
                    bytes[i] -= secret2[i % 20];

                if (i % 3 == 0)
                    bytes[i] += secret1[(i * 2) % 20];

                bytes[i] -= secret1[(bytes.Length - i) % 20];
            }
        }

        public static byte[] Encode(string str, byte[] extra)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(str); //this returns bytes with spaces between, like 84, 0, 104, 0, 105, 0 since it's 16-bit instead of 8-bit

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] += secret1[i % 20];

                if (i % 2 == 0)
                    bytes[i] += secret2[i % 20];
                else
                    bytes[i] -= secret2[i % 20];
            }

            if (extra != null && extra.Length > 0)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (i % 2 == 0)
                        bytes[i] -= extra[i % extra.Length];
                    else
                        bytes[i] += extra[i % extra.Length];
                }
            }

            //create unique key
            byte[] unique = new byte[3];
            new Random().NextBytes(unique);

            byte[] final = new byte[bytes.Length + 6];
            for (int i = 0; i < bytes.Length; i++)
            {
                final[i] = bytes[i];

                //if (i % 2 == 0)
                //    final[i] += unique[i / 2 % 3];
                //else
                //    final[i] -= unique[i / 2 % 3];
            }

            for (int i = 0; i < bytes.Length; i++)
            {
                int swap = unique[i % 3] % bytes.Length;

                byte temp = final[swap];
                final[swap] = final[i];
                final[i] = temp;
            }

            //attach the unique
            for (int i = 0; i < 3; i++)
                final[final.Length - 6 + i * 2] = unique[i]; //leave the 0's between

            return final;
        }

        public static string EncodeToString(string str)
        {
            return EncodeToString(str, null);
        }

        public static string EncodeToString(string str, byte[] extra)
        {
            byte[] salted = Encode(str, extra);
            return Encoding.Unicode.GetString(salted, 0, salted.Length);
        }

        public static string Decode(string str)
        {
            return Decode(str, null);
        }

        public static string Decode(string str, byte[] extra)
        {
            return Decode(Encoding.Unicode.GetBytes(str), extra);
        }

        public static string Decode(byte[] bytes, byte[] extra)
        {
            //grab the unique
            byte[] unique = new byte[3];
            for (int i = 0; i < 3; i++)
                unique[i] = bytes[bytes.Length - 6 + i * 2]; //account for the spaces in betweeen

            //undo the unique
            byte[] normal = new byte[bytes.Length - 6];
            for (int i = 0; i < normal.Length; i++)
            {
                normal[i] = bytes[i];

                //if (i % 2 == 0)
                //    normal[i] -= unique[i / 2 % 3];
                //else
                //    normal[i] += unique[i / 2 % 3];
            }

            for (int i = normal.Length - 1; i >= 0; i--)
            {
                int swap = unique[i % 3] % normal.Length;

                byte temp = normal[swap];
                normal[swap] = normal[i];
                normal[i] = temp;
            }

            if (extra != null && extra.Length > 0)
            {
                for (int i = 0; i < normal.Length; i++)
                {
                    if (i % 2 == 0)
                        normal[i] += extra[i % extra.Length];
                    else
                        normal[i] -= extra[i % extra.Length];
                }
            }

            for (int i = 0; i < normal.Length; i++)
            {
                if (i % 2 == 0)
                    normal[i] -= secret2[i % 20];
                else
                    normal[i] += secret2[i % 20];

                normal[i] -= secret1[i % 20];
            }

            return Encoding.Unicode.GetString(normal, 0, normal.Length);
        }
    }
}
