using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace VSL
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

            byte* nn = stackalloc byte[4];
            fixed (byte* buffer = buf)
            {
                if (BitConverter.IsLittleEndian)
                {
                    *(ushort*)nn = *(ushort*)(buffer + startIdx);
                    *(nn + 2) = *(buffer + startIdx + 2);
                }
                else
                {
                    *(ushort*)(nn + 1) = *(ushort*)(buffer + startIdx);
                    *(nn + 3) = *(buffer + startIdx + 2);
                }
                return *(uint*)nn;
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

            byte* nn = (byte*)&n;
            // This cast is necessary as (*uint)ptr + 1 is equal to &((uint*)ptr[1])
            // which means we would actually skip sizeof(uint) instead of 1

            fixed (byte* buffer = buf)
            {
                if (BitConverter.IsLittleEndian)
                {
                    *(ushort*)(buffer + startIdx) = *(ushort*)nn;
                    *(buffer + startIdx + 2) = *(nn + 2);
                }
                else
                {
                    *(ushort*)(buffer + startIdx) = *(ushort*)(nn + 1);
                    *(buffer + startIdx + 2) = *(nn + 3);
                }
            }
        }
    }
}