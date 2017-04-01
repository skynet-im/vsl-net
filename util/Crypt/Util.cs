using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Crypt
{
    public static class Util
    {
        // v9 © 2017 Daniel Lerch
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
                else
                {
                    rb.Add(b);
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

        public static string BytesToHexString(byte[] b)
        {
            string result = "";
            foreach (byte sb in b)
            {
                result += sb.ToString("x2");
            }
            return result;
        }

        public static byte[] HexStringToBytes(string s)
        {
            if (s.Length % 2 != 0) throw new ArgumentException("String has to be formatted hexadecimally");
            List<byte> l = new List<byte>();
            for (int i = 0; i <= s.Length; i += 2)
            {
                string hx = Convert.ToString(s[i]) + Convert.ToString(s[i + 1]);
                l.Add(Convert.ToByte(hx, 16));
            }
            return l.ToArray();
        }
    }
}