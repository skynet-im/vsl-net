using System;
using System.Text;
using System.IO;
using VSL.BinaryTools;

namespace VSL
{
    /// <summary>
    /// A byte buffer to read and write basic datatypes from a binary context.
    /// </summary>
    public abstract class PacketBuffer : IDisposable
    {
        // © 2017 - 2019 Daniel Lerch
        /// <summary>
        /// Encoding for string operations.
        /// </summary>
        protected static readonly Encoding encoding = Encoding.UTF8;

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

        /// <summary>
        /// Returns the content of the underlying buffer.
        /// </summary>
        public abstract byte[] ToArray();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        #region byte array
        /// <summary>
        /// Reads an <see cref="uint"/> and then a byte array with the length of this <see cref="uint"/>.
        /// </summary>
        /// <exception cref="ArgumentException" />
        public byte[] ReadByteArray() => ReadByteArray((int)ReadUInt());
        /// <summary>
        /// Reads a byte array with the specified length.
        /// </summary>
        /// <param name="count">The count of bytes to read.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException" />
        public abstract byte[] ReadByteArray(int count);
        /// <summary>
        /// Writes a byte array to the buffer.
        /// </summary>
        /// <param name="buffer">Source byte array with data.</param>
        /// <param name="autosize"><c>true</c> to write a <see cref="uint"/> for length, otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public void WriteByteArray(byte[] buffer, bool autosize)
            => WriteByteArray(buffer, 0, buffer.Length, autosize);
        /// <summary>
        /// Writes a byte array limited by an <see cref="ArraySegment{T}"/> to the buffer.
        /// </summary>
        /// <param name="buffer">Byte array segment with data.</param>
        /// <param name="autosize"><c>true</c> to write a <see cref="uint"/> for length, otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public void WriteByteArray(ArraySegment<byte> buffer, bool autosize)
            => WriteByteArray(buffer.Array, buffer.Offset, buffer.Count, autosize);
        /// <summary>
        /// Writes a byte array to the buffer.
        /// </summary>
        /// <param name="buffer">Source byte array with data.</param>
        /// <param name="offset">Source offset where to start reading.</param>
        /// <param name="count">Source count how many bytes to write.</param>
        /// <param name="autosize"><c>true</c> to write a <see cref="uint"/> for length, otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public abstract void WriteByteArray(byte[] buffer, int offset, int count, bool autosize);
        #endregion
        #region boolean types
        /// <summary>
        /// Reads a boolean.
        /// </summary>
        public bool ReadBool() => ReadByte() == 1;
        /// <summary>
        /// Writes a boolean as a byte (0 for false and 1 for true).
        /// </summary>
        public void WriteBool(bool b) => WriteByte(b ? (byte)1 : (byte)0);
        #endregion
        #region integral types
        /// <summary>
        /// Reads an 8-bit unsigned integer.
        /// </summary>
        public abstract byte ReadByte();
        /// <summary>
        /// Writes an 8-bit unsigned integer.
        /// </summary>
        public abstract void WriteByte(byte b);

        /// <summary>
        /// Reads a 16-bit signed integer.
        /// </summary>
        public virtual short ReadShort()
        {
            return BitConverter.ToInt16(ReadByteArray(2), 0);
        }
        /// <summary>
        /// Writes a 16-bit signed integer.
        /// </summary>
        public virtual void WriteShort(short s)
        {
            WriteByteArray(BitConverter.GetBytes(s), false);
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer.
        /// </summary>
        public virtual ushort ReadUShort()
        {
            return BitConverter.ToUInt16(ReadByteArray(2), 0);
        }
        /// <summary>
        /// Writes a 16-bit unsigned integer.
        /// </summary>
        public virtual void WriteUShort(ushort s)
        {
            WriteByteArray(BitConverter.GetBytes(s), false);
        }

        /// <summary>
        /// Reads a 32-bit signed integer.
        /// </summary>
        public virtual int ReadInt()
        {
            return BitConverter.ToInt32(ReadByteArray(4), 0);
        }
        /// <summary>
        /// Writes a 32-bit signed integer.
        /// </summary>
        public virtual void WriteInt(int i)
        {
            WriteByteArray(BitConverter.GetBytes(i), false);
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer.
        /// </summary>
        public virtual uint ReadUInt()
        {
            return BitConverter.ToUInt32(ReadByteArray(4), 0);
        }
        /// <summary>
        /// Writes a 32-bit unsigned integer.
        /// </summary>
        public virtual void WriteUInt(uint i)
        {
            WriteByteArray(BitConverter.GetBytes(i), false);
        }

        /// <summary>
        /// Reads a 64-bit signed integer.
        /// </summary>
        public virtual long ReadLong()
        {
            return BitConverter.ToInt64(ReadByteArray(8), 0);
        }
        /// <summary>
        /// Writes a 64-bit signed integer.
        /// </summary>
        public virtual void WriteLong(long l)
        {
            WriteByteArray(BitConverter.GetBytes(l), false);
        }

        /// <summary>
        /// Reads a 64-bit unsigned integer.
        /// </summary>
        public virtual ulong ReadULong()
        {
            return BitConverter.ToUInt64(ReadByteArray(8), 0);
        }
        /// <summary>
        /// Writes a 64-bit unsigned integer.
        /// </summary>
        public virtual unsafe void WriteULong(ulong l)
        {
            WriteByteArray(BitConverter.GetBytes(l), false);
        }
        #endregion
        #region floating point types
        /// <summary>
        /// Reads a single-precision floating-point number.
        /// </summary>
        public virtual float ReadSingle()
        {
            return BitConverter.ToSingle(ReadByteArray(4), 0);
        }
        /// <summary>
        /// Writes a single-precision floating-point number.
        /// </summary>
        public virtual unsafe void WriteSingle(float f)
        {
            WriteByteArray(BitConverter.GetBytes(f), false);
        }

        /// <summary>
        /// Reads a double-precision floating-point number.
        /// </summary>
        public virtual double ReadDouble()
        {
            return BitConverter.ToDouble(ReadByteArray(8), 0);
        }
        /// <summary>
        /// Writes a double-precision floating-point number.
        /// </summary>
        public virtual unsafe void WriteDouble(double f)
        {
            WriteByteArray(BitConverter.GetBytes(f), false);
        }
        #endregion
        #region combined types
        /// <summary>
        /// Reads a <see cref="DateTime"/> in binary format from a <see cref="long"/>.
        /// </summary>
        public DateTime ReadDate()
        {
            return DateTime.FromBinary(ReadLong());
        }
        /// <summary>
        /// Writes a <see cref="DateTime"/> in binary expression as a <see cref="long"/>.
        /// </summary>
        public void WriteDate(DateTime d)
        {
            WriteLong(d.ToBinary());
        }

        /// <summary>
        /// Reads an <see cref="uint"/> and then a <see cref="string"/> with UTF-8-Encoding and the length of this <see cref="uint"/>.
        /// </summary>
        public virtual string ReadString()
        {
            return encoding.GetString(ReadByteArray());
        }
        /// <summary>
        /// Writes an <see cref="uint"/> as a length marker and then the specified <see cref="string"/> with UTF-8-Encoding.
        /// </summary>
        public virtual void WriteString(string s)
        {
            WriteByteArray(encoding.GetBytes(s), true);
        }

        /// <summary>
        /// Reads an universally unique identifier in big endian format.
        /// </summary>
        public Guid ReadUuid()
        {
            byte[] uuid = ReadByteArray(16);
            byte[] guid = new byte[16];

            guid[15] = uuid[15]; // hoist bounds checks

            guid[00] = uuid[03];
            guid[01] = uuid[02];
            guid[02] = uuid[01];
            guid[03] = uuid[00];

            guid[04] = uuid[05];
            guid[05] = uuid[04];

            guid[06] = uuid[07];
            guid[07] = uuid[06];

            guid[08] = uuid[08];
            guid[09] = uuid[09];
            guid[10] = uuid[10];
            guid[11] = uuid[11];
            guid[12] = uuid[12];
            guid[13] = uuid[13];
            guid[14] = uuid[14];

            return new Guid(guid);
        }

        /// <summary>
        /// Writes an universally unique identifier in big endian format.
        /// </summary>
        public void WriteUuid(Guid value)
        {
            byte[] guid = value.ToByteArray();
            byte[] uuid = new byte[16];

            uuid[15] = guid[15]; // hoist bounds checks

            uuid[00] = guid[03];
            uuid[01] = guid[02];
            uuid[02] = guid[01];
            uuid[03] = guid[00];

            uuid[04] = guid[05];
            uuid[05] = guid[04];

            uuid[06] = guid[07];
            uuid[07] = guid[06];

            uuid[08] = guid[08];
            uuid[09] = guid[09];
            uuid[10] = guid[10];
            uuid[11] = guid[11];
            uuid[12] = guid[12];
            uuid[13] = guid[13];
            uuid[14] = guid[14];

            WriteByteArray(uuid, false);
        }
        #endregion
    }
}