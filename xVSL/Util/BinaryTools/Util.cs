using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace VSL.BinaryTools
{
    /// <summary>
    /// Provides methods and extension for handling binary data.
    /// </summary>
    public static class Util
    {
        // © 2017 - 2018 Daniel Lerch
        private static unsafe uint* encodeTable;
        private static byte[] decodeTable;

        static unsafe Util()
        {
            encodeTable = (uint*)GCHandle.Alloc(CreateEncodeLookup32(), GCHandleType.Pinned).AddrOfPinnedObject();
            decodeTable = CreateDecodeLookup8();
        }
        #region Take/Skip Bytes
        /// <summary>
        /// Takes the specified count of bytes from a byte array starting at the first position. This function runs much faster than IEnumerable.Take(int).ToArray().
        /// </summary>
        /// <param name="source">The source array to take bytes from.</param>
        /// <param name="count">The count of bytes to take.</param>
        /// <returns>A new byte array containing the taken bytes.</returns>
        public static byte[] Take(this byte[] source, int count)
        {
            byte[] result = new byte[count];
            Array.Copy(source, result, count);
            return result;
        }

        /// <summary>
        /// Takes the specified count of bytes from a byte array starting at the specifed index. This function runs much faster than IEnumerable.Skip(int).Take(int).ToArray().
        /// </summary>
        /// <param name="source">The source array to take bytes from.</param>
        /// <param name="startIdx">The source index to start taking.</param>
        /// <param name="count">The count of bytes to take.</param>
        /// <returns>A new byte array containing the taken bytes.</returns>
        public static byte[] TakeAt(this byte[] source, int startIdx, int count)
        {
            byte[] result = new byte[count];
            Array.Copy(source, startIdx, result, 0, count);
            return result;
        }
        /// <summary>
        /// Skips the specified count of bytes of a byte array starting at the first position. This function runs much faster than IEnumerable.Skip(int).ToArray().
        /// </summary>
        /// <param name="source">The source array to skip bytes.</param>
        /// <param name="count">The count of bytes to skip.</param>
        /// <returns>A new byte array not containing the skipped bytes.</returns>
        public static byte[] Skip(this byte[] source, int count)
        {
            byte[] result = new byte[source.Length - count];
            Array.Copy(source, count, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// Skips the specified count of bytes of a byte array starting at the specified index. This function runs much faster than LINQ's IEnumerable extensions.
        /// </summary>
        /// <param name="source">The source array to skip bytes.</param>
        /// <param name="startIdx">The source index to start skipping.</param>
        /// <param name="count">The count of bytes to skip.</param>
        /// <returns>A new byte array not containing the skipped bytes.</returns>
        public static byte[] SkipAt(this byte[] source, int startIdx, int count)
        {
            byte[] result = new byte[source.Length - count];
            Array.Copy(source, result, startIdx);
            Array.Copy(source, startIdx + count, result, startIdx, result.Length - startIdx);
            return result;
        }
        #endregion
        #region Sequence Equal
        /// <summary>
        /// Determines whether two byte arrays are equal. This function is not resistant against timing attacks. It runs more than 10 times faster than IEnumerable.SequenceEqual(IEnumerable).
        /// </summary>
        /// <param name="left">First array to compare. If one array is null the method returns false.</param>
        /// <param name="right">Second array to compare. If one array is null the method returns false.</param>
        /// <returns>Whether both arrays are equal.</returns>
        public static unsafe bool FastEquals(this byte[] left, byte[] right)
        {
            if (left == null || right == null) return false;
            if (left.Length != right.Length) return false;
            if (left.Length == 0) return true;

            int len = left.Length;
            fixed (byte* leftPtr = left)
            fixed (byte* rightPtr = right)
            {
                byte* leftP = leftPtr;
                byte* rightP = rightPtr;
                while (len >= 8)
                {
                    if (*(ulong*)leftP != *(ulong*)rightP) return false;
                    leftP += 8;
                    rightP += 8;
                    len -= 8;
                }
                if (len >= 4)
                {
                    if (*(uint*)leftP != *(uint*)rightP) return false;
                    leftP += 4;
                    rightP += 4;
                    len -= 4;
                }
                if (len >= 2)
                {
                    if (*(ushort*)leftP != *(ushort*)rightP) return false;
                    leftP += 2;
                    rightP += 2;
                    len -= 2;
                }
                if (len >= 1)
                {
                    if (*leftP != *rightP) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines whether two byte arrays are equal. This function is resistant against timing attacks. It iterates till the end even if a difference has been found.
        /// </summary>
        /// <param name="left">First array to compare. If one array is null the method returns false.</param>
        /// <param name="right">Second array to compare. If one array is null the method returns false.</param>
        /// <returns>Whether both arrays are equal.</returns>
        public static unsafe bool SafeEquals(this byte[] left, byte[] right)
        {
            if (left == null || right == null) return false;
            if (left.Length != right.Length) return false;
            if (left.Length == 0) return true;

            int len = left.Length;
            bool result = true;
            fixed (byte* leftPtr = left)
            fixed (byte* rightPtr = right)
            {
                byte* leftP = leftPtr;
                byte* rightP = rightPtr;
                while (len >= 8)
                {
                    if (*(ulong*)leftP != *(ulong*)rightP) result = false;
                    leftP += 8;
                    rightP += 8;
                    len -= 8;
                }
                if (len >= 4)
                {
                    if (*(uint*)leftP != *(uint*)rightP) result = false;
                    leftP += 4;
                    rightP += 4;
                    len -= 4;
                }
                if (len >= 2)
                {
                    if (*(ushort*)leftP != *(ushort*)rightP) result = false;
                    leftP += 2;
                    rightP += 2;
                    len -= 2;
                }
                if (len >= 1)
                {
                    if (*leftP != *rightP) result = false;
                }
            }
            return result;
        }
        #endregion
        #region Concat/Split
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
        /// Splits a byte array into blocks.
        /// </summary>
        /// <param name="buffer">The source byte array to split.</param>
        /// <param name="blocksize">The blocksize that all blocks except the last one will have.</param>
        /// <returns></returns>
        public static byte[][] SplitBytes(byte[] buffer, int blocksize)
        {
            int length = buffer.Length;
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
                Array.Copy(buffer, i * blocksize, final[i], 0, blocksize);
            }
            int pending = length - i * blocksize;
            final[blocks - 1] = new byte[pending];
            Array.Copy(buffer, i * blocksize, final[blocks - 1], 0, pending);
            return final;
        }
        #endregion
        #region Total Size
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
        #endregion
        #region Hex String
        /// <summary>
        /// Converts a byte array to a hexadecimal string. This method uses a 32bit lookup table.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <remarks>https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/24343727#24343727</remarks>
        public static unsafe string ToHexString(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            uint* lookupP = encodeTable;
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
            byte[] result = new byte[hexadecimal.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = Convert.ToByte(hexadecimal.Substring(i * 2, 2), 16);
            return result;
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
                result[b] = (byte)((decode[hexadecimal[c]] << 4) | decode[hexadecimal[c + 1]]);
            return result;
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
                result[i] = (byte)(i - 48);
            for (int i = 65; i < 71; i++) // unicode chars A..F
                result[i] = (byte)(i - 65 + 10);
            for (int i = 97; i < 103; i++) // unicode chars a..f
                result[i] = (byte)(i - 97 + 10);
            return result;
        }
        #endregion
    }
}