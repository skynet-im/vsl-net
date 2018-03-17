using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Crypt
{
    /// <summary>
    /// Provides useful functions for cryptography
    /// </summary>
    // TODO: Move to VSL\Util\Util.cs
    public static class Util
    {
        // © 2017 Daniel Lerch
        static unsafe uint* _encodePtr;
        static byte[] decodeTable;
        [SecuritySafeCritical]
        static unsafe Util()
        {
            uint[] encode = CreateEncodeLookup32();
            _encodePtr = (uint*)GCHandle.Alloc(encode, GCHandleType.Pinned).AddrOfPinnedObject();
            decodeTable = CreateDecodeLookup8();
        }
        /// <summary>
        /// Splits a byte array into blocks.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="blocksize"></param>
        /// <returns></returns>
        public static byte[][] SplitBytes(byte[] b, int blocksize)
        {
            int length = b.Length;
            int blocks = GetTotalSize(length, blocksize) / blocksize;
            byte[][] final = new byte[blocks][];
            if (length == 0)
            {
                final[0] = new byte[0];
                return final;
            }
            int i;
            for (i = 0; i < blocks - 1; i++)
            {
                final[i] = new byte[blocksize];
                Array.Copy(b, i * blocksize, final[i], 0, blocksize);
            }
            int pending = length - i * blocksize;
            final[blocks - 1] = new byte[pending];
            Array.Copy(b, i * blocksize, final[blocks - 1], 0, pending);
            return final;
        }

        /// <summary>
        /// Concatenates multiple byte arrays to one.
        /// </summary>
        /// <param name="arrays">An array of byte arrays to concatenate.</param>
        /// <returns></returns>
        [Obsolete("Util.ConnectBytes(Byte[][]) is deprecated. Please use Util.ConcatBytes(Byte[][]) instead.", false)] // deprecated since 1.3
        public static byte[] ConnectBytes(params byte[][] arrays) => ConcatBytes(arrays);

        /// <summary>
        /// Concatenates multiple byte arrays to one.
        /// </summary>
        /// <param name="arrays">An array of byte arrays to concatenate.</param>
        /// <returns></returns>
        public static byte[] ConcatBytes(params byte[][] arrays)
        {
            int n = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                n += arrays[i].Length;
            }
            byte[] final = new byte[n];
            n = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                byte[] c = arrays[i];
                Array.Copy(c, 0, final, n, c.Length);
                n += c.Length;
            }
            return final;
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
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <returns></returns>
        public static byte[] TakeBytes(byte[] b, int count, int startIndex)
        {
            byte[] final = new byte[count];
            Array.Copy(b, startIndex, final, 0, count);
            return final;
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string. This method uses a 32bit lookup table.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <remarks>https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/24343727#24343727</remarks>
        [SecuritySafeCritical]
        public static unsafe string ToHexString(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            uint* lookupP = _encodePtr;
            string result = new string((char)0, buffer.Length * 2);
            fixed (byte* bytesP = buffer)
            fixed (char* resultP = result)
            {
                uint* resultP2 = (uint*)resultP;
                for (int i = 0; i < buffer.Length; i++)
                    resultP2[i] = lookupP[bytesP[i]];
            }
            return result;
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array
        /// </summary>
        /// <param name="hexadecimal">hexadecimal string to convert</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"/>
        public static byte[] GetBytes(string hexadecimal)
        {
            if (hexadecimal == null) throw new ArgumentNullException(nameof(hexadecimal));
            if (hexadecimal.Length % 2 != 0) throw new ArgumentException("String has to be formatted hexadecimally", nameof(hexadecimal));
            byte[] final = new byte[hexadecimal.Length / 2];
            for (int i = 0; i < final.Length; i++)
                final[i] = Convert.ToByte(hexadecimal.Substring(i * 2, 2), 16);
            return final;
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array. Invalid chars are ignored and converted to 0x00
        /// </summary>
        /// <param name="hexadecimal"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"/>
        public static byte[] GetBytesUnchecked(string hexadecimal)
        {
            if (hexadecimal == null) throw new ArgumentNullException(nameof(hexadecimal));
            if (hexadecimal.Length % 2 != 0) throw new ArgumentException("String has to be formatted hexadecimally", nameof(hexadecimal));
            byte[] result = new byte[hexadecimal.Length / 2];
            byte[] decode = decodeTable;
            for (int c = 0, b = 0; c < hexadecimal.Length; c += 2, b++)
            {
                result[b] = (byte)((decode[hexadecimal[c]] << 4) | decode[hexadecimal[c + 1]]);
            }
            return result;
        }

        /// <summary>
        /// Gets the total size if only full blocks are allowed.
        /// </summary>
        /// <param name="normalSize">The default size of the input data.</param>
        /// <param name="blockSize">The blocksize of the algorithm to apply on the data.</param>
        /// <returns></returns>
        public static int GetTotalSize(int normalSize, int blockSize)
        {
            int mod = normalSize % blockSize;
            if (mod > 0)
                return normalSize - mod + blockSize;
            else
                return normalSize;
        }

        /// <summary>
        /// Gets the total size if only full blocks are allowed.
        /// </summary>
        /// <param name="normalSize">The default size of the input data.</param>
        /// <param name="blockSize">The blocksize of the algorithm to apply on the data.</param>
        /// <returns></returns>
        public static long GetTotalSize(long normalSize, int blockSize)
        {
            long mod = normalSize % blockSize;
            if (mod > 0)
                return normalSize - mod + blockSize;
            else
                return normalSize;
        }

        private static uint[] CreateEncodeLookup32()
        {
            uint[] result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("x2");
                if (BitConverter.IsLittleEndian)
                    result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
                else
                    result[i] = ((uint)s[1]) + ((uint)s[0] << 16);
            }
            return result;
        }

        private static byte[] CreateDecodeLookup8()
        {
            byte[] result = new byte[256];
            for (int i = 48; i < 58; i++) // unicode chars 0..9
            {
                result[i] = (byte)(i - 48);
            }
            for (int i = 65; i < 71; i++) // unicode chars A..F
            {
                result[i] = (byte)(i - 65 + 10);
            }
            for (int i = 97; i < 103; i++) // unicode chars a..f
            {
                result[i] = (byte)(i - 97 + 10);
            }
            return result;
        }
    }
}