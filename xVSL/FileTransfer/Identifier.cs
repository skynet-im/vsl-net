﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Provides different identifiers for file transfers.
    /// </summary>
    public class Identifier
    {
        /// <summary>
        /// Gets the selected identification mode.
        /// </summary>
        public IdentificationMode Mode { get; }
        /// <summary>
        /// Gets the specified identifier in the specified type.
        /// </summary>
        public object ID { get; }
        /// <summary>
        /// Creates a new instance of the Identifier class.
        /// </summary>
        /// <param name="id">Unsigned 32-bit integer as identifier.</param>
        public Identifier(uint id)
        {
            Mode = IdentificationMode.UInt32;
            ID = id;
        }
        /// <summary>
        /// Creates a new instance of the Identifier class.
        /// </summary>
        /// <param name="id">Unsigned 64-bit integer as identifier.</param>
        public Identifier(ulong id)
        {
            Mode = IdentificationMode.UInt64;
            ID = id;
        }
        /// <summary>
        /// Creates a new instance of the Identifier class.
        /// </summary>
        /// <param name="id">Byte array with max. 65536 bytes length as identifier.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Identifier(byte[] id)
        {
            Mode = IdentificationMode.ByteArray;
            ID = id;
        }
        /// <summary>
        /// Creates a new instance of the Identifier class.
        /// </summary>
        /// <param name="id">String as identifier.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Identifier(string id)
        {
            Mode = IdentificationMode.ByteArray;
            ID = id;
        }
        /// <summary>
        /// Supported identifier types.
        /// </summary>
        public enum IdentificationMode : byte
        {
            /// <summary>
            /// Unsigned integer as identifier.
            /// </summary>
            UInt32,
            /// <summary>
            /// Unsigned long as identifier.
            /// </summary>
            UInt64,
            /// <summary>
            /// Byte array as identifier.
            /// </summary>
            ByteArray,
            /// <summary>
            /// String (UTF-8) as identifier.
            /// </summary>
            String
        }
        /// <summary>
        /// Reads an identifier from binary data.
        /// </summary>
        /// <param name="buf">PacketBuffer containing the binary identifer.</param>
        /// <returns></returns>
        public static Identifier FromBinary(PacketBuffer buf)
        {
            IdentificationMode Type = (IdentificationMode)buf.ReadByte();
            switch (Type)
            {
                case IdentificationMode.UInt32:
                    return new Identifier(buf.ReadUInt());
                case IdentificationMode.UInt64:
                    return new Identifier(buf.ReadULong());
                case IdentificationMode.ByteArray:
                    int length = buf.ReadUShort() + 1; // 0 bytes length is not valid
                    return new Identifier(buf.ReadByteArray(length));
                case IdentificationMode.String:
                    return new Identifier(buf.ReadString());
                default:
                    return null;
            }
        }
        /// <summary>
        /// Deserializes the identifier into an byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBinary()
        {
            PacketBuffer buf = new PacketBuffer();
            ToBinary(buf);
            return buf.ToArray();
        }
        /// <summary>
        /// Deserializes the identifier into binary data.
        /// </summary>
        /// <param name="buf">PacketBuffer to write data in.</param>
        /// <returns></returns>
        public void ToBinary(PacketBuffer buf)
        {
            buf.WriteByte((byte)Mode);
            switch (Mode)
            {
                case IdentificationMode.UInt32:
                    buf.WriteUInt((uint)ID);
                    break;
                case IdentificationMode.UInt64:
                    buf.WriteULong((ulong)ID);
                    break;
                case IdentificationMode.ByteArray:
                    byte[] id = (byte[])ID;
                    buf.WriteUShort(Convert.ToUInt16(id.Length));
                    buf.WriteByteArray(id, false);
                    break;
                case IdentificationMode.String:
                    buf.WriteString((string)ID);
                    break;
            }
        }
    }
}