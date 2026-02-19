using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class EncryptionHelper
    {
        /// <summary>
        /// Converts to hexadecimal lowercase
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ConvertToHex(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder(bytes.Length);

            for (int i = 0; i < bytes.Length; i++)
                builder.Append(bytes[i].ToString("x2"));

            return builder.ToString();
        }

        /// <summary>
        /// Uses UTF8
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ConvertToBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string Sha256(string str)
        {
            return hashSha256(getBytes(str));
        }

        private static byte[] getBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        private static byte[] getBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            return bytes;
        }

        private static string hashSha256(byte[] bytes)
        {
            using (var algorithm = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = algorithm.ComputeHash(bytes);
                return ConvertToHex(hash);
            }
        }

        private static string hashSha1(byte[] bytes)
        {
            using (var algorithm = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hash = algorithm.ComputeHash(bytes);
                return ConvertToHex(hash);
            }
        }

        public static string Sha256(byte[] bytes)
        {
            return hashSha256(bytes);
        }

        public static string Sha1(byte[] bytes)
        {
            return hashSha1(bytes);
        }

        public static string Sha1(Stream stream)
        {
            return hashSha1(getBytes(stream));
        }
    }
}
