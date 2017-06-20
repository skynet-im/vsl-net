﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VSL
{
    /// <summary>
    /// A byte buffer to read and write basic datatypes
    /// </summary>
    public class PacketBuffer : IDisposable
    {
        // © 2017 Daniel Lerch
        // <fields
        MemoryStream baseStream;
        UTF8Encoding encoding;
        //  fields>
        // <constructor
        /// <summary>
        /// Initializes a new instance of the PacketBuffer class.
        /// </summary>
        public PacketBuffer()
        {
            baseStream = new MemoryStream();
            encoding = new UTF8Encoding();
        }
        /// <summary>
        /// Initializes a new instance of the PacketBuffer class.
        /// </summary>
        /// <param name="buffer">byte array to initialize</param>
        public PacketBuffer(byte[] buffer)
        {
            baseStream = new MemoryStream(buffer);
            encoding = new UTF8Encoding();
        }
        //  constructor>
        // <properties
        /// <summary>
        /// Gets the length of the underlying MemoryStream in bytes.
        /// </summary>
        public int Length
        {
            get
            {
                return Convert.ToInt32(baseStream.Length);
            }
        }
        /// <summary>
        /// Gets the current position within the underlying MemoryStream.
        /// </summary>
        public int Position
        {
            get
            {
                return Convert.ToInt32(baseStream.Position);
            }
        }
        /// <summary>
        /// Gets the count of pending bytes in the underlying MemoryStream.
        /// </summary>
        public int Pending
        {
            get
            {
                return Convert.ToInt32(baseStream.Length - baseStream.Position);
            }
        }
        //  properties>
        // <functions
        /// <summary>
        /// Returns the content of the underlying MemoryStream as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return baseStream.ToArray();
        }
        /// <summary>
        /// Reads a byte array from the buffer
        /// </summary>
        /// <param name="count">count of bytes to read</param>
        /// <returns></returns>
        private byte[] ReadByteRaw(int count)
        {
            byte[] b = new byte[count];
            baseStream.Read(b, 0, count);
            return b;
        }
        /// <summary>
        /// Writes a byte array to the end of the buffer
        /// </summary>
        /// <param name="b">byte array to write</param>
        private void WriteByteRaw(byte[] b)
        {
            baseStream.Write(b, 0, b.Length);
        }
        //  <byte
#pragma warning disable CS1591 //Disables "Missing XML comment..."
        public byte ReadByte()
        {
            return ReadByteRaw(1)[0];
        }
        public void WriteByte(byte b)
        {
            WriteByteRaw(new byte[1] { b });
        }
        //   byte>
        //  <byte array
        public byte[] ReadByteArray(int count)
        {
            return ReadByteRaw(count);
        }
        public byte[] ReadByteArray()
        {
            int count = Convert.ToInt32(ReadUInt());
            return ReadByteRaw(count);
        }
        /// <summary>
        /// Writes a byte array to the end buffer
        /// </summary>
        /// <param name="b">Byte array to write</param>
        /// <param name="autosize">True to write a uint for length, otherwise false</param>
        public void WriteByteArray(byte[] b, bool autosize = true)
        {
            if (autosize)
                WriteUInt(Convert.ToUInt32(b.Length));
            WriteByteRaw(b);
        }
        //   byte array>
        //  <bool
        public bool ReadBool()
        {
            return ReadByteRaw(1)[0] == 1;
        }
        public void WriteBool(bool b)
        {
            WriteByteRaw(new byte[1] { Convert.ToByte(b ? 1 : 0) });
        }
        //   bool>
        //  <short
        public short ReadShort()
        {
            return BitConverter.ToInt16(ReadByteRaw(2), 0);
        }
        public void WriteShort(short s)
        {
            WriteByteRaw(BitConverter.GetBytes(s));
        }
        //   short>
        //  <ushort
        public ushort ReadUShort()
        {
            return BitConverter.ToUInt16(ReadByteRaw(2), 0);
        }
        public void WriteUShort(ushort s)
        {
            WriteByteRaw(BitConverter.GetBytes(s));
        }
        //   ushort>
        //  <int
        public int ReadInt()
        {
            return BitConverter.ToInt32(ReadByteRaw(4), 0);
        }
        public void WriteInt(int i)
        {
            WriteByteRaw(BitConverter.GetBytes(i));
        }
        //   int>
        //  <uint
        public uint ReadUInt()
        {
            return BitConverter.ToUInt32(ReadByteRaw(4), 0);
        }
        public void WriteUInt(uint i)
        {
            WriteByteRaw(BitConverter.GetBytes(i));
        }
        //   uint>
        //  <long
        public long ReadLong()
        {
            return BitConverter.ToInt64(ReadByteRaw(8), 0);
        }
        public void WriteLong(long l)
        {
            WriteByteRaw(BitConverter.GetBytes(l));
        }
        //   long>
        //  <ulong
        public ulong ReadULong()
        {
            return BitConverter.ToUInt64(ReadByteRaw(8), 0);
        }
        public void WriteULong(ulong l)
        {
            WriteByteRaw(BitConverter.GetBytes(l));
        }
        //   ulong>
        //  <date
        public DateTime ReadDate()
        {
            return DateTime.FromBinary(ReadLong());
        }
        public void WriteDate(DateTime d)
        {
            WriteLong(d.ToBinary());
        }
        //   date>
        //  <string
        public string ReadString()
        {
            int len = Convert.ToInt32(ReadUInt());
            return encoding.GetString(ReadByteRaw(len));
        }
        public void WriteString(string s)
        {
            byte[] buf = encoding.GetBytes(s);
            WriteUInt(Convert.ToUInt32(buf.Length));
            WriteByteRaw(buf);
        }
#pragma warning restore CS1591
        //   string>
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
                    // TODO: dispose managed state (managed objects).
                    baseStream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PacketBuffer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        //  functions>
    }
}