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
            int n = 0;
            for (int i = 0; i < b.Length; i++)
            {
                n += b[i].Length;
            }
            byte[] final = new byte[n];
            n = 0;
            for (int i = 0; i < b.Length; i++)
            {
                byte[] c = b[i];
                Array.Copy(c, 0, final, n, c.Length);
                n += c.Length;
            }
            return final;
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
        /// Skips the specified count of bytes at the front of a byte array. This function was designed to run more efficient than IEnumerable.Skip(int).ToArray().
        /// </summary>
        /// <param name="b">Source byte array.</param>
        /// <param name="count">Number of bytes to skip.</param>
        /// <returns></returns>
        public static byte[] SkipBytes(byte[] b, int count)
        {
            byte[] final = new byte[b.Length - count];
            Array.Copy(b, count, final, 0, final.Length);
            return final;
        }

        /// <summary>
        /// Takes the specified count of bytes from the front of a byte array. This function was designed to run more efficient than IEnumerable.Take(int).ToArray().
        /// </summary>
        /// <param name="b">Source byte array.</param>
        /// <param name="count">Number of bytes to take.</param>
        /// <returns></returns>
        public static byte[] TakeBytes(byte[] b, int count)
        {
            byte[] final = new byte[count];
            Array.Copy(b, final, count);
            return final;
        }
        /// <summary>
        /// Takes the specified count of bytes from a byte array. This function was designed to run more efficient than IEnumerable.Take(int).ToArray().
        /// </summary>
        /// <param name="b">Source byte array.</param>
        /// <param name="count">Number of bytes to take.</param>
        /// <param name="startIndex">Index in the source byte array where taking starts.</param>
        /// <returns></returns>
        public static byte[] TakeBytes(byte[] b, int count, int startIndex)
        {
            byte[] final = new byte[count];
            Array.Copy(b, startIndex, final, 0, count);
            return final;
        }

        /// <summary>
        /// Determines whether two byte arrays are equal.
        /// </summary>
        /// <param name="b1">First byte array.</param>
        /// <param name="b2">Second byte array.</param>
        /// <returns></returns>
        public static bool ByteArraysEqual(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length) return false;
            if (b1.Length == 0) return true;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string
        /// </summary>
        /// <param name="b">byte array to convert</param>
        /// <returns></returns>
        public static string ToHexString(byte[] b)
        {
            string result = "";
            foreach (byte sb in b)
            {
                result += sb.ToString("x2");
            }
            return result;
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array
        /// </summary>
        /// <param name="hexadecimal">hexadecimal string to convert</param>
        /// <returns></returns>
        public static byte[] GetBytes(string hexadecimal)
        {
            if (hexadecimal.Length % 2 != 0) throw new ArgumentException("String has to be formatted hexadecimally");
            byte[] final = new byte[hexadecimal.Length / 2];
            for (int i = 0; i < final.Length; i++)
            {
                string hx = Convert.ToString(hexadecimal[i * 2]) + Convert.ToString(hexadecimal[i * 2 + 1]);
                final[i] = Convert.ToByte(hx, 16);
            }
            return final;
        }
    }
}