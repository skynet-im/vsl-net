using System;
using System.Security.Cryptography;
using VSL.BinaryTools;

namespace VSL.Crypt
{
    /// <summary>
    /// The simple implementation of AES in VSL
    /// </summary>
    public static class AesStatic
    {
        private static Aes aes;

        static AesStatic()
        {
            aes = Aes.Create();
        }
        #region public low-level API
        /// <summary>
        /// Executes an AES encryption.
        /// </summary>
        /// <param name="buffer">Plaintext.</param>
        /// <param name="key">AES key (128 or 256 bit).</param>
        /// <param name="iv">Initialization vector (128 bit).</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] buffer, byte[] key, byte[] iv)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (iv == null) throw new ArgumentNullException(nameof(iv));
            if (key.Length != 16 && key.Length != 32) throw new ArgumentOutOfRangeException(nameof(key), key.Length * 8, "The key must have a length of 128 or 256 bit");
            if (iv.Length != 16) throw new ArgumentOutOfRangeException(nameof(iv), iv.Length * 8, "The initialization vector must have a length of 128 bit");

            using (ICryptoTransform transform = aes.CreateEncryptor(key, iv))
            {
                return ProcessData(buffer, transform);
            }
        }

        /// <summary>
        /// Executes an AES encryption.
        /// </summary>
        /// <param name="buffer">Plaintext.</param>
        /// <param name="key">AES key (128 or 256 bit).</param>
        /// <param name="iv">Initialization vector (128 bit).</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <returns></returns>
        public static ArraySegment<byte> Encrypt(ArraySegment<byte> buffer, byte[] key, byte[] iv)
        {
            if (buffer.Array == null) throw new ArgumentNullException(nameof(buffer));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (iv == null) throw new ArgumentNullException(nameof(iv));
            if (key.Length != 16 && key.Length != 32) throw new ArgumentOutOfRangeException(nameof(key), key.Length * 8, "The key must have a length of 128 or 256 bit");
            if (iv.Length != 16) throw new ArgumentOutOfRangeException(nameof(iv), iv.Length * 8, "The initialization vector must have a length of 128 bit");

            using (ICryptoTransform transform = aes.CreateEncryptor(key, iv))
            {
                return ProcessData(buffer, transform);
            }
        }

        /// <summary>
        /// Executes an AES decryption.
        /// </summary>
        /// <param name="buffer">Ciphertext.</param>
        /// <param name="key">AES key (128 or 256 bit).</param>
        /// <param name="iv">Initialization vector (128 bit).</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] buffer, byte[] key, byte[] iv)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (iv == null) throw new ArgumentNullException(nameof(iv));
            if (buffer.Length % 16 != 0) throw new ArgumentOutOfRangeException(nameof(buffer), "The input must have a valid block size");
            if (key.Length != 16 && key.Length != 32) throw new ArgumentOutOfRangeException(nameof(key), key.Length * 8, "The key must have a length of 128 or 256 bit");
            if (iv.Length != 16) throw new ArgumentOutOfRangeException(nameof(iv), iv.Length * 8, "The initialization vector must have a length of 128 bit");

            using (ICryptoTransform transform = aes.CreateDecryptor(key, iv))
            {
                return ProcessData(buffer, transform);
            }
        }

        /// <summary>
        /// Executes an AES decryption.
        /// </summary>
        /// <param name="buffer">Ciphertext.</param>
        /// <param name="key">AES key (128 or 256 bit).</param>
        /// <param name="iv">Initialization vector (128 bit).</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        /// <returns></returns>
        public static ArraySegment<byte> Decrypt(ArraySegment<byte> buffer, byte[] key, byte[] iv)
        {
            if (buffer.Array == null) throw new ArgumentNullException(nameof(buffer));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (iv == null) throw new ArgumentNullException(nameof(iv));
            if (buffer.Count % 16 != 0) throw new ArgumentOutOfRangeException(nameof(buffer), "The input must have a valid block size");
            if (key.Length != 16 && key.Length != 32) throw new ArgumentOutOfRangeException(nameof(key), key.Length * 8, "The key must have a length of 128 or 256 bit");
            if (iv.Length != 16) throw new ArgumentOutOfRangeException(nameof(iv), iv.Length * 8, "The initialization vector must have a length of 128 bit");

            using (ICryptoTransform transform = aes.CreateDecryptor(key, iv))
            {
                return ProcessData(buffer, transform);
            }
        }

        /// <summary>
        /// Generates a new 256 bit AES key.
        /// </summary>
        public static byte[] GenerateKey() => GenerateRandom(32);

        /// <summary>
        /// Generates a new 128 bit initialization vector.
        /// </summary>
        public static byte[] GenerateIV() => GenerateRandom(16);

        /// <summary>
        /// Increments the little endian value of an array by one.
        /// </summary>
        /// <param name="iv"></param>
        public static unsafe void IncrementIV(byte[] iv)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(iv);

            fixed (byte* ptr = iv)
            {
                *(long*)ptr = *(long*)ptr + 1;
            }

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(iv);
        }
        #endregion
        #region private low-level API
        private static byte[] ProcessData(byte[] buffer, ICryptoTransform transform)
        {
            int inlen = buffer.Length;
            if (buffer.Length > 16)
            {
                byte[] first = new byte[inlen - inlen % 16];
                int outlen = transform.TransformBlock(buffer, 0, first.Length, first, 0);
                byte[] last = transform.TransformFinalBlock(buffer, first.Length, inlen - first.Length);
                byte[] final = new byte[outlen + last.Length];
                Array.Copy(first, final, outlen);
                Array.Copy(last, 0, final, outlen, last.Length);
                return final;
            }
            else
            {
                return transform.TransformFinalBlock(buffer, 0, inlen);
            }
        }

        private static ArraySegment<byte> ProcessData(ArraySegment<byte> buffer, ICryptoTransform transform)
        {
            if (buffer.Count > 16)
            {
                int first = buffer.Count - buffer.Count % 16;
                byte[] output = new byte[first + 16];
                int outlen = transform.TransformBlock(buffer.Array, buffer.Offset, first, output, 0);
                byte[] last = transform.TransformFinalBlock(buffer.Array, buffer.Offset + first, buffer.Count - first);
                Array.Copy(last, 0, output, outlen, last.Length);
                outlen += last.Length;
                return new ArraySegment<byte>(output, 0, outlen);
            }
            else
            {
                return new ArraySegment<byte>(transform.TransformFinalBlock(buffer.Array, buffer.Offset, buffer.Count));
            }
        }

        private static byte[] GenerateRandom(uint length)
        {
            byte[] random = new byte[length];
#if WINDOWS_UWP
            DataReader.FromBuffer(CryptographicBuffer.GenerateRandom(length)).ReadBytes(random);
#else
            using (RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider())
                csp.GetBytes(random);
#endif
            return random;
        }
        #endregion
        #region public high-level API
        /// <summary>
        /// Encrypts data to a <see cref="PacketBuffer"/> using AES-256 CBC and HMAC-SHA-256.
        /// </summary>
        /// <param name="source">The binary data to encrypt.</param>
        /// <param name="target">The buffer to write encrypted data.</param>
        /// <param name="writeLength">Specifiy if this method should write the length on the buffer.</param>
        /// <param name="hmacKey">The 256 bit key used for HMAC.</param>
        /// <param name="aesKey">The 256 bit key to decrypt data.</param>
        /// <returns>The length of the whole block including iv and hmac.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        public static int EncryptWithHmac(byte[] source, PacketBuffer target, bool writeLength, byte[] hmacKey, byte[] aesKey)
        {
            // Check arguments
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (hmacKey == null) throw new ArgumentNullException(nameof(hmacKey));
            if (hmacKey.Length != 32) throw new ArgumentOutOfRangeException(nameof(hmacKey), hmacKey.Length, "An HMAC key must be 32 bytes in length.");
            if (aesKey == null) throw new ArgumentNullException(nameof(aesKey));
            if (aesKey.Length != 32) throw new ArgumentOutOfRangeException(nameof(aesKey), aesKey.Length, "An AES key must be 32 bytes in length.");
            // Write length
            int length = 32 + 16 + Util.GetTotalSize(source.Length + 1, 16); // 32 byte HMAC, 16 byte IV, 1 byte PKCS#7 padding
            if (writeLength) target.WriteUInt((uint)length);
            // Encrypt data
            byte[] iv = GenerateIV();
            byte[] ciphertext = Encrypt(source, aesKey, iv);
            // Compute HMAC
            using (var hmacCsp = new HMACSHA256(hmacKey))
                target.WriteByteArray(hmacCsp.ComputeHash(Util.ConcatBytes(iv, ciphertext)), false);
            // Write data
            target.WriteByteArray(iv, false);
            target.WriteByteArray(ciphertext, false);
            return length;
        }
        /// <summary>
        /// Decrypts data from a <see cref="PacketBuffer"/> using AES-256 CBC and HMAC-SHA-256.
        /// </summary>
        /// <param name="source">The source buffer to read encrypted data.</param>
        /// <param name="length">The length of the whole block including iv and hmac. Specify 0 to read the length and -1 to read the whole <see cref="PacketBuffer"/>.</param>
        /// <param name="hmacKey">The 256 bit key used for HMAC.</param>
        /// <param name="aesKey">The 256 bit key to decrypt data.</param>
        /// <returns>The decrypted plaintext data.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        public static byte[] DecryptWithHmac(PacketBuffer source, int length, byte[] hmacKey, byte[] aesKey)
        {
            // Check arguments
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (length < -1) throw new ArgumentOutOfRangeException(nameof(length), length, "The length must no be lower than -1.");
            if (hmacKey == null) throw new ArgumentNullException(nameof(hmacKey));
            if (hmacKey.Length != 32) throw new ArgumentOutOfRangeException(nameof(hmacKey), hmacKey.Length, "An HMAC key must be 32 bytes in length.");
            if (aesKey == null) throw new ArgumentNullException(nameof(aesKey));
            if (aesKey.Length != 32) throw new ArgumentOutOfRangeException(nameof(aesKey), aesKey.Length, "An AES key must be 32 bytes in length.");
            // Read length
            int outerLength;
            if (length == -1)
                outerLength = source.Pending;
            else if (length == 0)
                outerLength = (int)source.ReadUInt();
            else
                outerLength = length;
            if (outerLength < 48) // 32 bytes (HMAC) + 16 bytes (IV)
                throw new ArgumentOutOfRangeException(nameof(length), outerLength, "The provided length must be at least 48 bytes.");
            if (outerLength % 16 != 0)
                throw new ArgumentOutOfRangeException(nameof(length), outerLength, "The provided length must be 16 byte large blocks.");
            // Read data
            byte[] hmac = source.ReadByteArray(32);
            byte[] iv = source.ReadByteArray(16);
            byte[] ciphertext = source.ReadByteArray(outerLength - 48);
            // Verify integrity
            using (var hmacCsp = new HMACSHA256(hmacKey))
                if (!hmac.SafeEquals(hmacCsp.ComputeHash(Util.ConcatBytes(iv, ciphertext))))
                    throw new CryptographicException("Message Corrupted: The HMAC values are not equal. The encrypted block may be tampered.");
            // Decrypt data
            return Decrypt(ciphertext, aesKey, iv);
        }
        #endregion
    }
}