using PCLCrypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    /// <summary>
    /// Note that you must add the PCLCrypto NuGet library to your core hosting platform app, so that the platform-specific implementation is loaded.
    /// </summary>
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
            return hash(str, HashAlgorithm.Sha256);
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

        private static string hash(string str, HashAlgorithm algorithmName)
        {
            return hash(getBytes(str), algorithmName);
        }

        private static string hash(Stream stream, HashAlgorithm algorithmName)
        {
            return hash(getBytes(stream), algorithmName);
        }

        private static string hash(byte[] bytes, HashAlgorithm algorithmName)
        {
            //grab the algoritm
            IHashAlgorithmProvider algorithm = WinRTCrypto.HashAlgorithmProvider.OpenAlgorithm(algorithmName);

            //hash the data
            byte[] hash = algorithm.HashData(bytes);

            //verify that hash succeeded
            if (hash.Length != algorithm.HashLength)
                throw new Exception("There was an error creating the hash.");

            //convert to string
            return WinRTCrypto.CryptographicBuffer.EncodeToHexString(hash);
        }

        public static string Sha256(byte[] bytes)
        {
            return hash(bytes, HashAlgorithm.Sha256);
        }

        public static string Sha1(byte[] bytes)
        {
            return hash(bytes, HashAlgorithm.Sha1);
        }

        public static string Sha1(Stream stream)
        {
            return hash(stream, HashAlgorithm.Sha1);
        }
    }
}
