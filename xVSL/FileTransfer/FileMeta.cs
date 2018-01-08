using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VSL.Crypt;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Provides meta data about files that were or will be transfered using VSL.
    /// </summary>
    public class FileMeta
    {
        /// <summary>
        /// Gets the original algorithm of this file.
        /// </summary>
        public ContentAlgorithm Algorithm { get; private set; }
        /// <summary>
        /// Gets whether plain data is available and the properties and fields can be used.
        /// </summary>
        public bool Available { get; private set; }
        /// <summary>
        /// Gets encrypted or unencrypted binary data that can be restored.
        /// </summary>
        public byte[] BinaryData { get; private set; }

        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public ulong Length { get; private set; }
        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public FileAttributes Attributes { get; private set; }
        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public DateTime CreationTime { get; private set; }
        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public DateTime LastAccessTime { get; private set; }
        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public DateTime LastWriteTime { get; private set; }
        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public byte[] Thumbnail { get; private set; }
        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public byte[] SHA256 { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMeta"/> class from binary data.
        /// </summary>
        /// <param name="binaryData"></param>
        /// <param name="connectionVersion"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public FileMeta(byte[] binaryData, ushort connectionVersion)
        {
            if (binaryData == null) throw new ArgumentNullException("binaryData");
            if (binaryData.Length < 76) throw new ArgumentOutOfRangeException("binaryData", "A valid v1.1 FileHeader packet must contain at least 76 bytes.");

            if (connectionVersion > 1)
                Read_v1_2(new PacketBuffer(binaryData));
            else
                Read_v1_1(new PacketBuffer(binaryData));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMeta"/> class with expected <see cref="ContentAlgorithm.Aes256CbcHmacSha256"/>.
        /// </summary>
        /// <param name="binaryData"></param>
        /// <param name="hmacKey"></param>
        /// <param name="aesKey"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        public FileMeta(byte[] binaryData, byte[] hmacKey, byte[] aesKey)
        {
            if (binaryData == null) throw new ArgumentNullException("binaryData");
            if (binaryData.Length < 77) throw new ArgumentOutOfRangeException("binaryData", "A valid v1.2 FileHeader packet must contain at least 77 bytes.");
            if (hmacKey == null) throw new ArgumentNullException("hmacKey");
            if (hmacKey.Length != 32) throw new ArgumentOutOfRangeException("hmacKey", "An HMAC key must be 32 bytes in length.");
            if (aesKey == null) throw new ArgumentNullException("aesKey");
            if (aesKey.Length != 32) throw new ArgumentOutOfRangeException("aesKey", "An AES key must be 32 bytes in length.");

            Read_v1_2(new PacketBuffer(binaryData), hmacKey, aesKey);
        }

        private void Read_v1_1(PacketBuffer buf)
        {
            Algorithm = ContentAlgorithm.None;
            BinaryData = buf.ToArray();
            Name = buf.ReadString();
            Length = buf.ReadULong();
            Attributes = (FileAttributes)buf.ReadUInt();
            CreationTime = buf.ReadDate();
            LastAccessTime = buf.ReadDate();
            LastWriteTime = buf.ReadDate();
            Thumbnail = buf.ReadByteArray();
            SHA256 = buf.ReadByteArray(32);
            Available = true;
        }

        private void Read_v1_2(PacketBuffer buf)
        {
            Algorithm = (ContentAlgorithm)buf.ReadByte();
            BinaryData = Util.SkipBytes(buf.ToArray(), 1);
            if (Algorithm == ContentAlgorithm.None)
                Read_v1_2_Core(buf);
        }

        private void Read_v1_2(PacketBuffer buf, byte[] hmacKey, byte[] aesKey)
        {
            Algorithm = (ContentAlgorithm)buf.ReadByte();
            BinaryData = Util.SkipBytes(buf.ToArray(), 1);
            if (Algorithm == ContentAlgorithm.None)
                Read_v1_2_Core(buf);
            else if (Algorithm == ContentAlgorithm.Aes256CbcHmacSha256)
            {
                byte[] hmac = buf.ReadByteArray(32);
                using (HMACSHA256 csp = new HMACSHA256(hmacKey))
                {
                    int pos = buf.Position;
                    int pending = buf.Pending;
                    if (!hmac.SequenceEqual(csp.ComputeHash(buf.ReadByteArray(pending))))
                        throw new CryptographicException("MessageCorrupted: The integrity checking resulted in a corrupted message.");
                    buf.Position = pos;
                    byte[] iv = buf.ReadByteArray(16);
                }
            }
        }

        private void Read_v1_2_Core(PacketBuffer buf)
        {

        }
    }
}