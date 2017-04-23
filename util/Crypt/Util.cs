using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Crypt
{
    /// <summary>
    /// Provides useful functions for cryptography
    /// </summary>
    public static class Util
    {
        // © 2017 Daniel Lerch
        /// <summary>
        /// Splits a byte array into blocks
        /// </summary>
        /// <param name="b"></param>
        /// <param name="blocksize"></param>
        /// <returns></returns>
        public static byte[][] SplitBytes(byte[] b, int blocksize)
        {
            List<byte[]> rb = new List<byte[]>();
            while (true)
            {
                if (b.Length >= blocksize)
                {
                    rb.Add(b.Take(blocksize).ToArray());
                    b = b.Skip(blocksize).ToArray();
                }
                else if (b.Length > 0)
                {
                    rb.Add(b);
                    break;
                }
                else
                {
                    break;
                }
            }
            return rb.ToArray();
        }

        /// <summary>
        /// Connects mutiple byte arrays to one
        /// </summary>
        /// <param name="b">two-dimensional byte array to connect</param>
        /// <returns></returns>
        public static byte[] ConnectBytes(byte[][] b)
        {
            byte[] buf = new byte[0];
            foreach (byte[] ba in b)
            {
                buf = buf.Concat(ba).ToArray();
            }
            return buf;
        }
        /// <summary>
        /// Connects multiple byte arrays to one
        /// </summary>
        /// <param name="b">byte arrays to connect</param>
        /// <returns></returns>
        public static byte[] ConnectBytesPA(params byte[][] b)
        {
            return ConnectBytes(b);
        }
        /// <summary>
        /// Converts a byte array to a hexadecimal string
        /// </summary>
        /// <param name="b">byte array to convert</param>
        /// <returns></returns>
        public static string GetHexString(byte[] b)
        {
            string result = "";
            foreach (byte sb in b)
            {
                result += sb.ToString("x2");
            }
            return result;
        }
        /// <summary>
        /// Converts a byte array to a hexadecimal string
        /// </summary>
        /// <param name="b">byte array to convert</param>
        /// <returns></returns>
        [Obsolete("Util.BytesToHexString(byte[]) is deprecated, please use Util.GetHexString(byte[]) instead.")]
        public static string BytesToHexString(byte[] b)
        {
            return GetHexString(b);
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array
        /// </summary>
        /// <param name="hexadecimal">hexadecimal string to convert</param>
        /// <returns></returns>
        public static byte[] GetBytes(string hexadecimal)
        {
            if (hexadecimal.Length % 2 != 0) throw new ArgumentException("String has to be formatted hexadecimally");
            List<byte> l = new List<byte>();
            for (int i = 0; i < hexadecimal.Length - 1; i += 2)
            {
                string hx = Convert.ToString(hexadecimal[i]) + Convert.ToString(hexadecimal[i + 1]);
                l.Add(Convert.ToByte(hx, 16));
            }
            return l.ToArray();
        }
        /// <summary>
        /// Converts a hexadecimal string to a byte array
        /// </summary>
        /// <param name="s">hexadecimal string to convert</param>
        /// <returns></returns>
        [Obsolete("Util.HexStringToBytes(string) is deprecated, please use Util.GetBytes(string) instead.")]
        public static byte[] HexStringToBytes(string s)
        {
            return GetBytes(s);
        }
    }
}