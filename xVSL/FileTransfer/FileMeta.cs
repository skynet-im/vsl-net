using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VSL.Crypt;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Provides meta data about files that were or will be transfered using VSL.
    /// </summary>
    public class FileMeta
    {
        private byte[] encryptedContent;
        /// <summary>
        /// Gets the cryptographic algorithm of this <see cref="FileMeta"/>.
        /// </summary>
        public ContentAlgorithm Algorithm { get; private set; }
        /// <summary>
        /// Gets the length of the file. If it is encrypted the length of the encrypted content is used.
        /// </summary>
        public long Length { get; private set; }
        /// <summary>
        /// Gets whether plain data is available and the properties and fields can be used.
        /// </summary>
        public bool Available { get; private set; }

        #region encrypted properties
        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public byte[] HmacKey { get; private set; }
        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public byte[] AesKey { get; private set; }
        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public string Name { get; private set; }
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
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public ContentAlgorithm FileEncryption { get; private set; } = ContentAlgorithm.None;
        /// <summary>
        /// This property can only be used if <see cref="Available"/> returns true.
        /// </summary>
        public byte[] FileKey { get; private set; }
        #endregion
        #region read from packet
        /// <summary>
        /// Initializes a new instance of the <see cref="FileMeta"/> class from binary data.
        /// </summary>
        /// <param name="binaryData"></param>
        /// <param name="connectionVersion"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="NotSupportedException"/>
        public FileMeta(byte[] binaryData, ushort connectionVersion)
        {
            if (binaryData == null) throw new ArgumentNullException("binaryData");
            if (binaryData.Length < 76) throw new ArgumentOutOfRangeException("binaryData", "A valid v1.1 FileHeader packet must contain at least 76 bytes.");
            if (connectionVersion < Constants.CompatibilityVersion || connectionVersion > Constants.VersionNumber)
                throw new NotSupportedException($"VSL {Constants.ProductVersion(4)} only support connection versions from {Constants.CompatibilityVersion} to {Constants.VersionNumber} but not {connectionVersion}");

            if (connectionVersion == 1)
                Read_v1_1(new PacketBuffer(binaryData));
            else if (connectionVersion == 2)
                Read_v1_2(new PacketBuffer(binaryData));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMeta"/> class with <see cref="ContentAlgorithm.Aes256CbcHmacSha256"/>.
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
            Name = buf.ReadString();
            Length = Convert.ToInt64(buf.ReadULong());
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
            Read_v1_2_Header(buf);
            if (Algorithm == ContentAlgorithm.None)
            {
                Read_v1_2_Core(buf);
                Available = true;
            }
            else
                encryptedContent = buf.ReadByteArray(buf.Pending);
        }

        private void Read_v1_2(PacketBuffer buf, byte[] hmacKey, byte[] aesKey)
        {
            Read_v1_2_Header(buf);
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
                    HmacKey = hmacKey;
                }
                byte[] iv = buf.ReadByteArray(16);
                byte[] plain = AesStatic.Decrypt(buf.ReadByteArray(buf.Pending), aesKey, iv);
                AesKey = aesKey;
                using (PacketBuffer innerBuf = new PacketBuffer(plain))
                    Read_v1_2_Core(innerBuf);
                Available = true;
            }
            else
                encryptedContent = buf.ReadByteArray(buf.Pending);
        }

        private void Read_v1_2_Header(PacketBuffer buf)
        {
            Algorithm = (ContentAlgorithm)buf.ReadByte();
            Length = Convert.ToInt64(buf.ReadULong());
            FileEncryption = (ContentAlgorithm)buf.ReadByte();
        }

        private void Read_v1_2_Core(PacketBuffer buf)
        {
            Name = buf.ReadString();
            Attributes = (FileAttributes)buf.ReadUInt();
            CreationTime = buf.ReadDate();
            LastAccessTime = buf.ReadDate();
            LastWriteTime = buf.ReadDate();
            Thumbnail = buf.ReadByteArray();
            SHA256 = buf.ReadByteArray(32);
            if (FileEncryption == ContentAlgorithm.Aes256Cbc)
                FileKey = buf.ReadByteArray(32);
        }
        #endregion
        /// <summary>
        /// Decrypts an encrypted <see cref="FileMeta"/> with <see cref="ContentAlgorithm.Aes256CbcHmacSha256"/>.
        /// </summary>
        /// <param name="hmacKey"></param>
        /// <param name="aesKey"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public void Decrypt(byte[] hmacKey, byte[] aesKey)
        {
            if (hmacKey == null) throw new ArgumentNullException("hmacKey");
            if (hmacKey.Length != 32) throw new ArgumentOutOfRangeException("hmacKey", "An HMAC key must be 32 bytes in length.");
            if (aesKey == null) throw new ArgumentNullException("aesKey");
            if (aesKey.Length != 32) throw new ArgumentOutOfRangeException("aesKey", "An AES key must be 32 bytes in length.");

            using (PacketBuffer buf = new PacketBuffer(GetBinaryData(Constants.VersionNumber)))
                Read_v1_2(buf, hmacKey, aesKey);
        }
        #region read from file
        /// <summary>
        /// Initializes a new instance of the <see cref="FileMeta"/> class and generates all required keys.
        /// </summary>
        /// <param name="path">The local file path to load the meta data from.</param>
        /// <param name="algorithm">The cryptographic algorith to encrypt this <see cref="FileMeta"/>.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="NotSupportedException"/>
        public FileMeta(string path, ContentAlgorithm algorithm) : this(path, algorithm, null, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMeta"/> class and generates missing keys.
        /// </summary>
        /// <param name="path">The local file path to load the meta data from.</param>
        /// <param name="algorithm">The cryptographic algorith to encrypt this <see cref="FileMeta"/>.</param>
        /// <param name="hmacKey">256 bit HMAC key to verify integrity of this <see cref="FileMeta"/>.</param>
        /// <param name="aesKey">256 bit AES key to encrypt this <see cref="FileMeta"/>.</param>
        /// <param name="fileKey">256 bit AES key to encrypt the associated file.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="FileNotFoundException"/>
        public FileMeta(string path, ContentAlgorithm algorithm, byte[] hmacKey, byte[] aesKey, byte[] fileKey)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");
            if (algorithm != ContentAlgorithm.None && algorithm != ContentAlgorithm.Aes256CbcHmacSha256)
                throw new NotSupportedException("This content algorithm is not supported.");

            Algorithm = algorithm;
            AesKey = aesKey;
            HmacKey = hmacKey;
            FileKey = fileKey;

            if (Algorithm == ContentAlgorithm.Aes256CbcHmacSha256)
            {
                if (AesKey == null)
                    AesKey = AesStatic.GenerateKey();
                else if (AesKey.Length != 32)
                    throw new ArgumentOutOfRangeException("aesKey");

                if (hmacKey == null)
                    HmacKey = AesStatic.GenerateKey();
                else if (HmacKey.Length != 32)
                    throw new ArgumentOutOfRangeException("hmacKey");

                if (fileKey == null)
                    FileKey = AesStatic.GenerateKey();
                else if (FileKey.Length != 32)
                    throw new ArgumentOutOfRangeException("fileKey");

                FileEncryption = ContentAlgorithm.Aes256Cbc; // The file needs no HMAC as we have an SHA256
            }

            LoadFromFile(path);
            Available = true;
        }

        /// <summary>
        /// Returns the binary expression of this <see cref="FileMeta"/> like it will be sent over the internet.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public byte[] GetBinaryData(ushort version)
        {
            using (PacketBuffer buf = new PacketBuffer())
            {
                if (version == 1)
                    Write_v1_1(buf);
                else
                {
                    if (Algorithm == ContentAlgorithm.None)
                    {
                        Write_v1_2_Header(buf);
                        Write_v1_2_Core(buf);
                    }
                    else if (Algorithm == ContentAlgorithm.Aes256CbcHmacSha256)
                    {
                        Write_v1_2_Header(buf); // write header anyway because we always these data

                        if (Available)
                        {
                            byte[] plaindata;
                            using (PacketBuffer ibuf = new PacketBuffer())
                            {
                                Write_v1_2_Core(ibuf);
                                plaindata = ibuf.ToArray();
                            }

                            byte[] iv = AesStatic.GenerateIV();
                            byte[] ciphertext = AesStatic.Encrypt(plaindata, AesKey, iv); // pre-compute cipher block for HMAC

                            using (HMACSHA256 hmac = new HMACSHA256(HmacKey))
                                buf.WriteByteArray(hmac.ComputeHash(Util.ConnectBytes(iv, ciphertext)), false); // compute and write HMAC of iv and ciphertext
                            buf.WriteByteArray(iv, false);
                            buf.WriteByteArray(ciphertext, false);
                        }
                        else
                            buf.WriteByteArray(encryptedContent, false); // write all pre-read encrypted content including hmac, iv, etc.
                    }
                }
                return buf.ToArray();
            }
        }

        /// <summary>
        /// Returns the binary expression of this <see cref="FileMeta"/> without any encryption, hmacs, ivs, etc.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public byte[] GetPlainData(ushort version)
        {
            using (PacketBuffer buf = new PacketBuffer())
            {
                if (version == 1)
                    Write_v1_1(buf);
                else
                {
                    Write_v1_2_Header(buf);
                    Write_v1_2_Core(buf);
                }
                return buf.ToArray();
            }
        }

        private void LoadFromFile(string path)
        {
            FileInfo fi = new FileInfo(path);
            if (!fi.Exists) throw new FileNotFoundException("File to load data could not be found.", path);
            byte[] hash = null;
            Task hashT = Task.Run(() => hash = Hash.SHA256(path));
            Name = fi.Name;
            if (FileEncryption == ContentAlgorithm.None)
                Length = fi.Length;
            else if (FileEncryption == ContentAlgorithm.Aes256Cbc)
                Length = Util.GetTotalSize(fi.Length + 17, 16); // 16 bytes iv + 1 byte 
            Attributes = fi.Attributes;
            CreationTime = fi.CreationTime;
            LastAccessTime = fi.LastAccessTime;
            LastWriteTime = fi.LastWriteTime;
            Thumbnail = new byte[0]; // Thumbnails aren't supported yet
            hashT.Wait();
            SHA256 = hash;
        }

        private void Write_v1_1(PacketBuffer buf)
        {
            Algorithm = ContentAlgorithm.None;
            buf.WriteString(Name);
            buf.WriteULong(Convert.ToUInt64(Length));
            buf.WriteUInt((uint)Attributes);
            buf.WriteDate(CreationTime);
            buf.WriteDate(LastAccessTime);
            buf.WriteDate(LastWriteTime);
            buf.WriteByteArray(Thumbnail);
            buf.WriteByteArray(SHA256, false);
        }

        private void Write_v1_2_Header(PacketBuffer buf)
        {
            buf.WriteByte((byte)Algorithm);
            buf.WriteULong(Convert.ToUInt64(Length));
            buf.WriteByte((byte)FileEncryption);
        }

        private void Write_v1_2_Core(PacketBuffer buf)
        {
            buf.WriteString(Name);
            buf.WriteUInt((uint)Attributes);
            buf.WriteDate(CreationTime);
            buf.WriteDate(LastAccessTime);
            buf.WriteDate(LastWriteTime);
            buf.WriteByteArray(Thumbnail);
            buf.WriteByteArray(SHA256, false);
            if (FileEncryption == ContentAlgorithm.Aes256Cbc)
                buf.WriteByteArray(FileKey, false);
        }
        #endregion

        /// <summary>
        /// Applies the meta data of this <see cref="FileMeta"/> to a file.
        /// </summary>
        /// <param name="sourcePath">The path where file is currently stored.</param>
        /// <param name="targetDir">The target directory where the file will be moved.</param>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <returns></returns>
        public string Apply(string sourcePath, string targetDir)
        {
            if (!Available)
                throw new InvalidOperationException("You can only apply the FileMeta information if it is decrypted.");
            if (!Directory.Exists(targetDir))
                throw new DirectoryNotFoundException("The specified target directory could not be found.");

            FileInfo current = new FileInfo(sourcePath)
            {
                Attributes = Attributes,
                CreationTime = CreationTime,
                LastAccessTime = LastAccessTime,
                LastWriteTime = LastWriteTime
            };
            if (File.Exists(Path.Combine(targetDir, Name)))
            {
                ulong count = 2;
                string name = Name.Remove(Name.LastIndexOf('.'));
                string extension = Name.Substring(Name.LastIndexOf('.') + 1);
                while (true)
                {
                    string newpath = Path.Combine(targetDir, name + " (" + count + ")." + extension);
                    if (File.Exists(newpath))
                        count++;
                    else
                    {
                        current.MoveTo(newpath);
                        break;
                    }
                }
            }
            else
                current.MoveTo(Path.Combine(targetDir, Name));
            return current.FullName;
        }
    }
}