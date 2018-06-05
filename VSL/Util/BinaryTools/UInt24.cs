using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace VSL.BinaryTools
{
    internal static class UInt24
    {
        internal static uint FromBytes(byte[] buf)
        {
            if (buf == null)
                throw new ArgumentNullException(nameof(buf));
            if (buf.Length < 3)
                throw new ArgumentOutOfRangeException(nameof(buf), buf.Length, "The buffer must have a size of at least 3 bytes");
            return FromBytes(buf, 0);
        }

        [SecuritySafeCritical]
        internal unsafe static uint FromBytes(byte[] buf, int startIdx)
        {
            if (buf == null)
                throw new ArgumentNullException(nameof(buf));
            if (startIdx < 0)
                throw new ArgumentOutOfRangeException(nameof(startIdx), startIdx, "The start index must not be negative.");
            if (startIdx > buf.Length - 3) // prevent buffer overflow
                throw new ArgumentOutOfRangeException(nameof(startIdx), startIdx,
                    "The start index must be small enough to keep three bytes remaining.");

            byte* array = stackalloc byte[4];
            fixed (byte* buffer = buf)
            {
                if (BitConverter.IsLittleEndian)
                {
                    *(ushort*)array = *(ushort*)(buffer + startIdx);
                    *(array + 2) = *(buffer + startIdx + 2);
                }
                else
                {
                    *(ushort*)(array + 1) = *(ushort*)(buffer + startIdx);
                    *(array + 3) = *(buffer + startIdx + 2);
                }
                return *(uint*)array;
            }
        }

        internal static byte[] ToBytes(uint n)
        {
            byte[] final = new byte[3];
            ToBytes(n, final, 0);
            return final;
        }

        [SecuritySafeCritical]
        internal unsafe static void ToBytes(uint n, byte[] buf, int startIdx)
        {
            if (buf == null)
                throw new ArgumentNullException(nameof(buf));
            if (startIdx < 0)
                throw new ArgumentOutOfRangeException(nameof(startIdx), startIdx, "The start index must not be negative.");
            if (startIdx > buf.Length - 3) // prevent buffer overflow
                throw new ArgumentOutOfRangeException(nameof(startIdx), startIdx,
                    "The start index must be small enough to keep three bytes remaining.");

            uint* nn = &n;
            fixed (byte* array = buf)
            {
                if (BitConverter.IsLittleEndian)
                {
                    *(ushort*)(array + startIdx) = *(ushort*)nn;
                    *(array + startIdx + 2) = *(byte*)(nn + 2);
                }
                else
                {
                    *(ushort*)(array + startIdx) = *(ushort*)(nn + 1);
                    *(array + startIdx + 2) = *(byte*)(nn + 3);
                }
            }
        }
    }
}