using System;
using System.Collections.Generic;
using System.Text;

namespace VSL
{
    internal static class UInt24
    {
        internal static uint FromBytes(byte[] buf)
        {
            if (buf == null)
                throw new ArgumentNullException("buf");
            if (buf.Length < 3)
                throw new ArgumentOutOfRangeException("buf");
            //return (((uint)(buf[0])) & 0x0000FF) |
            //    (((uint)(buf[1]) << 8) & 0x00FF00) |
            //    (((uint)(buf[2]) << 16) & 0xFF0000);
            return FromBytes(buf, 0);
        }

        internal unsafe static uint FromBytes(byte[] buf, int startIdx)
        {
            if (buf == null)
                throw new ArgumentNullException("buf");
            if (buf.Length < 3)
                throw new ArgumentOutOfRangeException("buf");
            byte[] tmp = new byte[4];
            fixed (byte* array = tmp)
            {
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
        }

        internal static byte[] ToBytes(uint n)
        {
            //return new byte[] { (byte)n, (byte)(n >> 8), (byte)(n >> 16) };
            byte[] final = new byte[3];
            ToBytes(n, final, 0);
            return final;
        }

        internal unsafe static void ToBytes(uint n, byte[] buf, int startIdx)
        {
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