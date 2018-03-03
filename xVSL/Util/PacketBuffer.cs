using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace VSL
{
    /// <summary>
    /// A byte buffer to read and write basic datatypes from a binary context.
    /// </summary>
    public sealed class PacketBuffer : IDisposable
    {
        // © 2017 - 2018 Daniel Lerch
        // <fields
        private int position;

        private byte[] baseBuffer;
        private GCHandle baseHandle;
        private unsafe byte* basePtr;

        private MemoryStream baseStream;
        private static readonly Encoding encoding = Encoding.UTF8;
        //  fields>
        // <constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketBuffer"/> class over a new <see cref="MemoryStream"/>.
        /// </summary>
        public PacketBuffer()
        {
            baseStream = new MemoryStream();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketBuffer"/> class over a byte array with the specified length.
        /// </summary>
        /// <param name="size"></param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        [SecuritySafeCritical]
        public unsafe PacketBuffer(int size)
        {
            if (size < 0) throw new ArgumentOutOfRangeException("size");
            baseBuffer = new byte[size];
            baseHandle = GCHandle.Alloc(baseBuffer, GCHandleType.Pinned);
            basePtr = (byte*)baseHandle.AddrOfPinnedObject();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketBuffer"/> class with the specified data in read mode.
        /// </summary>
        /// <param name="buffer">byte array to initialize</param>
        /// <exception cref="ArgumentNullException"/>
        [SecuritySafeCritical]
        public unsafe PacketBuffer(byte[] buffer)
        {
            baseBuffer = buffer ?? throw new ArgumentNullException("buffer");
            baseHandle = GCHandle.Alloc(baseBuffer, GCHandleType.Pinned);
            basePtr = (byte*)baseHandle.AddrOfPinnedObject();
        }
        //  constructor>
        // <properties
        /// <summary>
        /// Gets the length of the underlying buffer.
        /// </summary>
        public int Length => baseBuffer?.Length ?? (int)baseStream.Length;

        /// <summary>
        /// Gets the current position within the underlying buffer.
        /// </summary>
        public int Position
        {
            get => position;
            set
            {
                position = value;
                if (baseStream != null) baseStream.Position = value;
            }
        }

        /// <summary>
        /// Gets the count of pending bytes in the underlying buffer.
        /// </summary>
        public int Pending => Length - Position;
        //  properties>
        // <functions
        /// <summary>
        /// Returns the content of the underlying buffer.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return baseBuffer ?? baseStream.ToArray();
        }
        /// <summary>
        /// Writes a byte array to the buffer and increases the position.
        /// </summary>
        /// <param name="b">byte array to write</param>
        private void WriteByteRaw(byte[] b) => WriteByteRaw(b, 0, b.Length);
        /// <summary>
        /// Writes a byte array to the buffer and increases the position.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        private void WriteByteRaw(byte[] buffer, int offset, int count)
            => baseStream.Write(buffer, offset, count);

        #region public byte array
        //  <byte array
        /// <summary>
        /// Reads a byte array with the specified length.
        /// </summary>
        /// <param name="count">The count of bytes to read.</param>
        /// <returns></returns>
        public byte[] ReadByteArray(int count)
        {
            byte[] buffer = new byte[count];
            Array.Copy(baseBuffer, position, buffer, 0, count);
            position += count;
            return buffer;
        }
        /// <summary>
        /// Reads an <see cref="uint"/> and then a byte array with the length of this <see cref="uint"/>.
        /// </summary>
        /// <returns></returns>
        public byte[] ReadByteArray() => ReadByteArray((int)ReadUInt());
        /// <summary>
        /// Reads a byte array with the specified length without increasing the source index.
        /// </summary>
        /// <param name="count">The count of bytes to read.</param>
        /// <returns></returns>
        public byte[] PeekByteArray(int count)
        {
            byte[] buffer = new byte[count];
            Array.Copy(baseBuffer, position, buffer, 0, count);
            return buffer;
        }
        /// <summary>
        /// Reads an <see cref="uint"/> and then a bytes array with the length of this <see cref="uint"/> without increasing the source index.
        /// </summary>
        /// <returns></returns>
        public byte[] PeekByteArray()
        {
            int count = (int)BitConverter.ToUInt32(baseBuffer, position);
            byte[] buffer = new byte[count];
            Array.Copy(baseBuffer, position += 4, buffer, 0, count);
            return buffer;
        }
        /// <summary>
        /// Writes a byte array with autosize = true.
        /// </summary>
        /// <param name="buffer"></param>
        [Obsolete("PacketBuffer.WriteByteArray(byte[]) is deprecated. Please use PacketBuffer.WriteByteArray(byte[], bool) instead.", false)] // deprecated since v1.2.2
        public void WriteByteArray(byte[] buffer) => WriteByteArray(buffer, true);
        /// <summary>
        /// Writes a byte array to the buffer.
        /// </summary>
        /// <param name="buffer">Source byte array with data.</param>
        /// <param name="autosize">True to write a <see cref="uint"/> for length, otherwise false.</param>
        public void WriteByteArray(byte[] buffer, bool autosize)
            => WriteByteArray(buffer, 0, buffer.Length, autosize);
        /// <summary>
        /// Writes a byte array to the buffer.
        /// </summary>
        /// <param name="buffer">Source byte array with data.</param>
        /// <param name="offset">Source offset where to start reading.</param>
        /// <param name="count">Source count how many bytes to write.</param>
        /// <param name="autosize">True to write a <see cref="uint"/> for length, otherwise false.</param>
        public void WriteByteArray(byte[] buffer, int offset, int count, bool autosize)
        {
            if (baseBuffer != null)
            {
                if (Pending < count + (autosize ? 4 : 0)) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
                if (autosize) WriteUInt((uint)count);
                Array.Copy(buffer, offset, baseBuffer, position, count);
                position += count;
            }
            else
            {
                if (autosize) WriteUInt((uint)count);
                WriteByteRaw(buffer, offset, count);
            }
        }
        //   byte array>
        #endregion
        #region boolean types
        //  <bool
        /// <summary>
        /// Reads a boolean.
        /// </summary>
        /// <returns></returns>
        public bool ReadBool() => ReadByte() == 1;
        /// <summary>
        /// Writes a boolean as a byte (0 for false and 1 for true).
        /// </summary>
        /// <param name="b"></param>
        public void WriteBool(bool b) => WriteByte(b ? (byte)1 : (byte)0);
        //   bool>
        #endregion
        #region integral types
        //  <byte
        /// <summary>
        /// Reads an 8-bit unsigned integer.
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            byte value = baseBuffer[position];
            position++;
            return value;
        }
        /// <summary>
        /// Writes an 8-bit unsigned integer.
        /// </summary>
        /// <param name="b"></param>
        public void WriteByte(byte b)
        {
            if (baseBuffer != null)
            {
                if (Pending < 1) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
                baseBuffer[position] = b;
                position++;
            }
            else
            {
                baseStream.WriteByte(b);
                position++;
            }
        }
        //   byte>
        //  <short
        /// <summary>
        /// Reads a 16-bit signed integer.
        /// </summary>
        /// <returns></returns>
        public short ReadShort()
        {
            short value = BitConverter.ToInt16(baseBuffer, position);
            position += 2;
            return value;
        }
        /// <summary>
        /// Writes a 16-bit signed integer.
        /// </summary>
        /// <param name="s"></param>
        [SecuritySafeCritical]
        public unsafe void WriteShort(short s)
        {
            if (baseBuffer != null)
            {
                if (Pending < 2) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
                *(short*)(basePtr + position) = s;
                position += 2;
            }
            else
                WriteByteRaw(BitConverter.GetBytes(s));
        }
        //   short>
        //  <ushort
        /// <summary>
        /// Reads a 16-bit unsigned integer.
        /// </summary>
        /// <returns></returns>
        public ushort ReadUShort()
        {
            ushort value = BitConverter.ToUInt16(baseBuffer, position);
            position += 2;
            return value;
        }
        /// <summary>
        /// Writes a 16-bit unsigned integer.
        /// </summary>
        /// <param name="s"></param>
        [SecuritySafeCritical]
        public unsafe void WriteUShort(ushort s)
        {
            if (baseBuffer != null)
            {
                if (Pending < 2) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
                *(ushort*)(basePtr + position) = s;
                position += 2;
            }
            else
                WriteByteRaw(BitConverter.GetBytes(s));
        }
        //   ushort>
        //  <int
        /// <summary>
        /// Reads a 32-bit signed integer.
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            int value = BitConverter.ToInt32(baseBuffer, position);
            position += 4;
            return value;
        }
        /// <summary>
        /// Writes a 32-bit signed integer.
        /// </summary>
        /// <param name="i"></param>
        [SecuritySafeCritical]
        public unsafe void WriteInt(int i)
        {
            if (baseBuffer != null)
            {
                if (Pending < 4) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
                *(int*)(basePtr + position) = i;
                position += 4;
            }
            else
                WriteByteRaw(BitConverter.GetBytes(i));
        }
        //   int>
        //  <uint
        /// <summary>
        /// Reads a 32-bit unsigned integer.
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt()
        {
            uint value = BitConverter.ToUInt32(baseBuffer, position);
            position += 4;
            return value;
        }
        /// <summary>
        /// Writes a 32-bit unsigned integer.
        /// </summary>
        /// <param name="i"></param>
        [SecuritySafeCritical]
        public unsafe void WriteUInt(uint i)
        {
            if (baseBuffer != null)
            {
                if (Pending < 4) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
                *(uint*)(basePtr + position) = i;
                position += 4;
            }
            else
                WriteByteRaw(BitConverter.GetBytes(i));
        }
        //   uint>
        //  <long
        /// <summary>
        /// Reads a 64-bit signed integer.
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            long value = BitConverter.ToInt64(baseBuffer, position);
            position += 8;
            return value;
        }
        /// <summary>
        /// Writes a 64-bit signed integer.
        /// </summary>
        /// <param name="l"></param>
        [SecuritySafeCritical]
        public unsafe void WriteLong(long l)
        {
            if (baseBuffer != null)
            {
                if (Pending < 8) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
                *(long*)(basePtr + position) = l;
                position += 8;
            }
            else
                WriteByteRaw(BitConverter.GetBytes(l));
        }
        //   long>
        //  <ulong
        /// <summary>
        /// Reads a 64-bit unsigned integer.
        /// </summary>
        /// <returns></returns>
        public ulong ReadULong()
        {
            ulong value = BitConverter.ToUInt64(baseBuffer, position);
            position += 8;
            return value;
        }
        /// <summary>
        /// Writes a 64-bit unsigned integer.
        /// </summary>
        /// <param name="l"></param>
        [SecuritySafeCritical]
        public unsafe void WriteULong(ulong l)
        {
            if (baseBuffer != null)
            {
                if (Pending < 8) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
                *(ulong*)(basePtr + position) = l;
                position += 8;
            }
            else
                WriteByteRaw(BitConverter.GetBytes(l));
        }
        //   ulong>
        #endregion
        #region floating point types
        //  <float
        /// <summary>
        /// Reads a single-precision floating-point number.
        /// </summary>
        /// <returns></returns>
        public float ReadSingle()
        {
            float value = BitConverter.ToSingle(baseBuffer, position);
            position += 4;
            return value;
        }
        /// <summary>
        /// Writes a single-precision floating-point number.
        /// </summary>
        /// <param name="f"></param>
        [SecuritySafeCritical]
        public unsafe void WriteSingle(float f)
        {
            if (baseBuffer != null)
            {
                if (Pending < 4) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
                *(float*)(basePtr + position) = f;
                position += 4;
            }
            else
                WriteByteRaw(BitConverter.GetBytes(f));
        }
        //   float>
        //  <double
        /// <summary>
        /// Reads a double-precision floating-point number.
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            double value = BitConverter.ToDouble(baseBuffer, position);
            position += 8;
            return value;
        }
        /// <summary>
        /// Writes a double-precision floating-point number.
        /// </summary>
        /// <param name="f"></param>
        [SecuritySafeCritical]
        public unsafe void WriteDouble(double f)
        {
            if (baseBuffer != null)
            {
                if (Pending < 8) throw new InvalidOperationException("The buffer is not big enough to perform this operation.");
                *(double*)(basePtr + position) = f;
                position += 8;
            }
            else
                WriteByteRaw(BitConverter.GetBytes(f));
        }
        //   double>
        #endregion
        #region combined types
        //  <date
        /// <summary>
        /// Reads a <see cref="DateTime"/> in binary format from a <see cref="long"/>.
        /// </summary>
        /// <returns></returns>
        public DateTime ReadDate()
        {
            return DateTime.FromBinary(ReadLong());
        }
        /// <summary>
        /// Writes a <see cref="DateTime"/> in binary expression as a <see cref="long"/>.
        /// </summary>
        /// <param name="d"></param>
        public void WriteDate(DateTime d)
        {
            WriteLong(d.ToBinary());
        }
        //   date>
        //  <string
        /// <summary>
        /// Reads an <see cref="uint"/> and then a <see cref="string"/> with UTF-8-Encoding and the length of this <see cref="uint"/>.
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            int len = (int)ReadUInt();
            string value = encoding.GetString(baseBuffer, position, len);
            position += len;
            return value;
        }
        /// <summary>
        /// Writes an <see cref="uint"/> as a length marker and then the specified <see cref="string"/> with UTF-8-Encoding.
        /// </summary>
        /// <param name="s"></param>
        public void WriteString(string s)
        {
            if (baseBuffer != null)
            {
                int len = encoding.GetBytes(s, 0, s.Length, baseBuffer, position += 4);
                WriteUInt((uint)len);
                position += len;
            }
            else
            {
                byte[] buf = encoding.GetBytes(s);
                WriteUInt(Convert.ToUInt32(buf.Length));
                WriteByteRaw(buf);
            }
        }
        //   string>
        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Specify whether managed objects should be disposed.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // -TODO: dispose managed state (managed objects).
                    if (baseStream != null)
                        baseStream.Dispose();
                }

                // -TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                if (baseBuffer != null)
                    baseHandle.Free();
                // -TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // -TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        /// <summary>
        /// Cleans up unmanaged resources.
        /// </summary>
        ~PacketBuffer()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // -TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
        //  functions>
    }
}