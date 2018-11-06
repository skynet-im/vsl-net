using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace VSL.BinaryTools
{
    internal class PacketBufferStatic : PacketBuffer
    {
        private int position;

        private byte[] baseBuffer;
        private GCHandle baseHandle;
        private readonly unsafe byte* basePtr;

        [SecuritySafeCritical]
        internal unsafe PacketBufferStatic(int size)
        {
            baseBuffer = new byte[size];
            baseHandle = GCHandle.Alloc(baseBuffer, GCHandleType.Pinned);
            basePtr = (byte*)baseHandle.AddrOfPinnedObject();
        }

        [SecuritySafeCritical]
        internal unsafe PacketBufferStatic(byte[] buffer)
        {
            baseBuffer = buffer;
            baseHandle = GCHandle.Alloc(baseBuffer, GCHandleType.Pinned);
            basePtr = (byte*)baseHandle.AddrOfPinnedObject();
        }

        public override int Length => baseBuffer.Length;
        public override int Position { get => position; set => position = value; }

        public override byte[] ToArray() => baseBuffer;

        #region byte array
        public override byte[] ReadByteArray(int count)
        {
            byte[] buffer = new byte[count];
            Array.Copy(baseBuffer, position, buffer, 0, count);
            position += count;
            return buffer;
        }
        public override void WriteByteArray(byte[] buffer, int offset, int count, bool autosize)
        {
            if (Pending < count + (autosize ? 4 : 0)) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
            if (autosize) WriteUInt((uint)count);
            Array.Copy(buffer, offset, baseBuffer, position, count);
            position += count;
        }
        #endregion
        #region integral types
        public override byte ReadByte()
        {
            byte value = baseBuffer[position];
            position++;
            return value;
        }
        public override void WriteByte(byte b)
        {
            if (Pending < 1) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
            baseBuffer[position] = b;
            position++;
        }
        public override short ReadShort()
        {
            short value = BitConverter.ToInt16(baseBuffer, position);
            position += 2;
            return value;
        }
        [SecuritySafeCritical]
        public override unsafe void WriteShort(short s)
        {
            if (Pending < 2) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
            *(short*)(basePtr + position) = s;
            position += 2;
        }
        public override ushort ReadUShort()
        {
            ushort value = BitConverter.ToUInt16(baseBuffer, position);
            position += 2;
            return value;
        }
        [SecuritySafeCritical]
        public override unsafe void WriteUShort(ushort s)
        {
            if (Pending < 2) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
            *(ushort*)(basePtr + position) = s;
            position += 2;
        }
        public override int ReadInt()
        {
            int value = BitConverter.ToInt32(baseBuffer, position);
            position += 4;
            return value;
        }
        [SecuritySafeCritical]
        public override unsafe void WriteInt(int i)
        {
            if (Pending < 4) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
            *(int*)(basePtr + position) = i;
            position += 4;
        }
        public override uint ReadUInt()
        {
            uint value = BitConverter.ToUInt32(baseBuffer, position);
            position += 4;
            return value;
        }
        [SecuritySafeCritical]
        public override unsafe void WriteUInt(uint i)
        {
            if (Pending < 4) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
            *(uint*)(basePtr + position) = i;
            position += 4;
        }
        public override long ReadLong()
        {
            long value = BitConverter.ToInt64(baseBuffer, position);
            position += 8;
            return value;
        }
        [SecuritySafeCritical]
        public override unsafe void WriteLong(long l)
        {
            if (Pending < 8) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
            *(long*)(basePtr + position) = l;
            position += 8;
        }
        public override ulong ReadULong()
        {
            ulong value = BitConverter.ToUInt64(baseBuffer, position);
            position += 8;
            return value;
        }
        [SecuritySafeCritical]
        public override unsafe void WriteULong(ulong l)
        {
            if (Pending < 8) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
            *(ulong*)(basePtr + position) = l;
            position += 8;
        }
        #endregion
        #region floating point types
        public override float ReadSingle()
        {
            float value = BitConverter.ToSingle(baseBuffer, position);
            position += 4;
            return value;
        }
        [SecuritySafeCritical]
        public override unsafe void WriteSingle(float f)
        {
            if (Pending < 4) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
            *(float*)(basePtr + position) = f;
            position += 4;
        }
        public override double ReadDouble()
        {
            double value = BitConverter.ToDouble(baseBuffer, position);
            position += 8;
            return value;
        }
        [SecuritySafeCritical]
        public override unsafe void WriteDouble(double f)
        {
            if (Pending < 8) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
            *(double*)(basePtr + position) = f;
            position += 8;
        }
        #endregion
        #region combined types
        public override string ReadString()
        {
            int len = (int)ReadUInt();
            string value = encoding.GetString(baseBuffer, position, len);
            position += len;
            return value;
        }
        public override void WriteString(string s)
        {
            int len = encoding.GetBytes(s, 0, s.Length, baseBuffer, position += 4);
            WriteUInt((uint)len);
            position += len;
        }
        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                baseHandle.Free();

                disposedValue = true;
            }
        }
        #endregion
    }
}
