using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using VSL.BinaryTools;

namespace VSL
{
    /// <summary>
    /// A byte buffer to read and write basic datatypes from a binary context.
    /// </summary>
    public abstract class PacketBuffer : IDisposable
    {
        // © 2017 - 2018 Daniel Lerch
        // <fields
        /// <summary>
        /// The <see cref="Encoding"/> used for string operations.
        /// </summary>
        protected static readonly Encoding encoding = Encoding.UTF8;
        //  fields>
        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketBuffer"/> class over a <see cref="MemoryStream"/>.
        /// </summary>
        public static PacketBuffer CreateDynamic()
        {
            return new PacketBufferDynamic();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketBuffer"/> class over a <see cref="MemoryStream"/> with the specified data.
        /// </summary>
        /// <param name="buffer">byte array to initialize</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        public static PacketBuffer CreateDynamic(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            return new PacketBufferDynamic(buffer);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketBuffer"/> class over a byte array with the specified length.
        /// </summary>
        /// <param name="size"></param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static PacketBuffer CreateStatic(int size)
        {
            if (size < 0) throw new ArgumentOutOfRangeException(nameof(size), size, "The requested size cannot be negative.");
            return new PacketBufferStatic(size);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketBuffer"/> class over a byte array with the specified data.
        /// </summary>
        /// <param name="buffer">byte array to initialize</param>
        /// <exception cref="ArgumentNullException"/>
        public static PacketBuffer CreateStatic(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            return new PacketBufferStatic(buffer);
        }
        #endregion
        // <properties
        /// <summary>
        /// Gets the length of the underlying buffer.
        /// </summary>
        public abstract int Length { get; }
        /// <summary>
        /// Gets the count of pending bytes in the underlying buffer.
        /// </summary>
        public virtual int Pending => Length - Position;
        /// <summary>
        /// Gets the current position within the underlying buffer.
        /// </summary>
        public abstract int Position { get; set; }
        //  properties>

        // <functions
        /// <summary>
        /// Returns the content of the underlying buffer.
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ToArray();

        #region byte array
        /// <summary>
        /// Reads an <see cref="uint"/> and then a byte array with the length of this <see cref="uint"/>.
        /// </summary>
        /// <returns></returns>
        public byte[] ReadByteArray() => ReadByteArray((int)ReadUInt());
        /// <summary>
        /// Reads a byte array with the specified length.
        /// </summary>
        /// <param name="count">The count of bytes to read.</param>
        /// <returns></returns>
        public abstract byte[] ReadByteArray(int count);
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
        public abstract void WriteByteArray(byte[] buffer, int offset, int count, bool autosize);
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
        public abstract byte ReadByte();
        /// <summary>
        /// Writes an 8-bit unsigned integer.
        /// </summary>
        /// <param name="b"></param>
        public abstract void WriteByte(byte b);
        //   byte>
        //  <short
        /// <summary>
        /// Reads a 16-bit signed integer.
        /// </summary>
        /// <returns></returns>
        public virtual short ReadShort()
        {
            return BitConverter.ToInt16(ReadByteArray(2), 0);
        }
        /// <summary>
        /// Writes a 16-bit signed integer.
        /// </summary>
        /// <param name="s"></param>
        public virtual void WriteShort(short s)
        {
            WriteByteArray(BitConverter.GetBytes(s), false);
        }
        //   short>
        //  <ushort
        /// <summary>
        /// Reads a 16-bit unsigned integer.
        /// </summary>
        /// <returns></returns>
        public virtual ushort ReadUShort()
        {
            return BitConverter.ToUInt16(ReadByteArray(2), 0);
        }
        /// <summary>
        /// Writes a 16-bit unsigned integer.
        /// </summary>
        /// <param name="s"></param>
        public virtual void WriteUShort(ushort s)
        {
            WriteByteArray(BitConverter.GetBytes(s), false);
        }
        //   ushort>
        //  <int
        /// <summary>
        /// Reads a 32-bit signed integer.
        /// </summary>
        /// <returns></returns>
        public virtual int ReadInt()
        {
            return BitConverter.ToInt32(ReadByteArray(4), 0);
        }
        /// <summary>
        /// Writes a 32-bit signed integer.
        /// </summary>
        /// <param name="i"></param>
        public virtual void WriteInt(int i)
        {
            WriteByteArray(BitConverter.GetBytes(i), false);
        }
        //   int>
        //  <uint
        /// <summary>
        /// Reads a 32-bit unsigned integer.
        /// </summary>
        /// <returns></returns>
        public virtual uint ReadUInt()
        {
            return BitConverter.ToUInt32(ReadByteArray(4), 0);
        }
        /// <summary>
        /// Writes a 32-bit unsigned integer.
        /// </summary>
        /// <param name="i"></param>
        public virtual void WriteUInt(uint i)
        {
            WriteByteArray(BitConverter.GetBytes(i), false);
        }
        //   uint>
        //  <long
        /// <summary>
        /// Reads a 64-bit signed integer.
        /// </summary>
        /// <returns></returns>
        public virtual long ReadLong()
        {
            return BitConverter.ToInt64(ReadByteArray(8), 0);
        }
        /// <summary>
        /// Writes a 64-bit signed integer.
        /// </summary>
        /// <param name="l"></param>
        public virtual void WriteLong(long l)
        {
            WriteByteArray(BitConverter.GetBytes(l), false);
        }
        //   long>
        //  <ulong
        /// <summary>
        /// Reads a 64-bit unsigned integer.
        /// </summary>
        /// <returns></returns>
        public virtual ulong ReadULong()
        {
            return BitConverter.ToUInt64(ReadByteArray(8), 0);
        }
        /// <summary>
        /// Writes a 64-bit unsigned integer.
        /// </summary>
        /// <param name="l"></param>
        [SecuritySafeCritical]
        public virtual unsafe void WriteULong(ulong l)
        {
            WriteByteArray(BitConverter.GetBytes(l), false);
        }
        //   ulong>
        #endregion
        #region floating point types
        //  <float
        /// <summary>
        /// Reads a single-precision floating-point number.
        /// </summary>
        /// <returns></returns>
        public virtual float ReadSingle()
        {
            return BitConverter.ToSingle(ReadByteArray(4), 0);
        }
        /// <summary>
        /// Writes a single-precision floating-point number.
        /// </summary>
        /// <param name="f"></param>
        public virtual unsafe void WriteSingle(float f)
        {
            WriteByteArray(BitConverter.GetBytes(f), false);
        }
        //   float>
        //  <double
        /// <summary>
        /// Reads a double-precision floating-point number.
        /// </summary>
        /// <returns></returns>
        public virtual double ReadDouble()
        {
            return BitConverter.ToDouble(ReadByteArray(8), 0);
        }
        /// <summary>
        /// Writes a double-precision floating-point number.
        /// </summary>
        /// <param name="f"></param>
        public virtual unsafe void WriteDouble(double f)
        {
            WriteByteArray(BitConverter.GetBytes(f), false);
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
        public virtual string ReadString()
        {
            return encoding.GetString(ReadByteArray());
        }
        /// <summary>
        /// Writes an <see cref="uint"/> as a length marker and then the specified <see cref="string"/> with UTF-8-Encoding.
        /// </summary>
        /// <param name="s"></param>
        public virtual void WriteString(string s)
        {
            WriteByteArray(encoding.GetBytes(s), true);
        }
        //   string>
        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Specify whether managed objects should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // -TODO: dispose managed state (managed objects).
                }

                // -TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
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