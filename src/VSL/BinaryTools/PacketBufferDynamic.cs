using System;
using System.IO;

namespace VSL.BinaryTools
{
    internal sealed class PacketBufferDynamic : PacketBuffer
    {
        private MemoryStream baseStream;

        internal PacketBufferDynamic()
        {
            baseStream = new MemoryStream();
        }

        internal PacketBufferDynamic(byte[] buffer)
        {
            baseStream = new MemoryStream(buffer, true);
        }

        public override int Length => (int)baseStream.Length;
        public override int Position { get => (int)baseStream.Position; set => baseStream.Position = value; }

        public override byte[] ToArray() => baseStream.ToArray();

        #region byte array
        public override byte[] ReadByteArray(int count)
        {
            byte[] buffer = new byte[count];
            if (baseStream.Read(buffer, 0, count) < count)
                throw new ArgumentException("The buffer is not big enough to perform this operation.");
            return buffer;
        }
        public override void WriteByteArray(byte[] buffer, int offset, int count, bool autosize)
        {
            if (autosize) WriteUInt((uint)count);
            baseStream.Write(buffer, offset, count);
        }
        #endregion
        #region integral types
        public override byte ReadByte()
        {
            int result = baseStream.ReadByte();
            if (result < 0) throw new InvalidOperationException();
            return (byte)result;
        }
        public override void WriteByte(byte b)
        {
            baseStream.WriteByte(b);
        }
        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    baseStream.Dispose();
                }

                disposedValue = true;
            }
        }

        // ~PacketBufferDynamic() {
        //   Dispose(false);
        // }

        public override void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
