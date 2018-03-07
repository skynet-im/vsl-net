using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
#endif

namespace VSL.Crypt
{
    /// <summary>
    /// The simple implementation of AES in VSL
    /// </summary>
    public static class AesStatic
    {
#if WINDOWS_UWP
        private static Aes aes;
#else
        private static AesCryptoServiceProvider aes;
#endif

        static AesStatic()
        {
#if WINDOWS_UWP
            aes = Aes.Create();
#else
            aes = new AesCryptoServiceProvider();
#endif
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
        public static byte[] Encrypt(byte[] buffer, byte[] key, byte[] iv)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (key == null) throw new ArgumentNullException("key");
            if (iv == null) throw new ArgumentNullException("iv");
            if (key.Length != 16 && key.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 128 or 256 bit");
            if (iv.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");

            return EncryptInternal(buffer, key, iv);
        }

        /// <summary>
        /// Executes an AES encryption asychronously
        /// </summary>
        /// <param name="buffer">Plaintext</param>
        /// <param name="key">AES key (128 or 256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <returns></returns>
        public static Task<byte[]> EncryptAsync(byte[] buffer, byte[] key, byte[] iv)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (key == null) throw new ArgumentNullException("key");
            if (iv == null) throw new ArgumentNullException("iv");
            if (key.Length != 16 && key.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 128 or 256 bit");
            if (iv.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");

            return Task.Run(() => EncryptInternal(buffer, key, iv));
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
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (key == null) throw new ArgumentNullException("key");
            if (iv == null) throw new ArgumentNullException("iv");
            if (buffer.Length % 16 != 0) throw new ArgumentOutOfRangeException("buffer", "The input must have a valid block size");
            if (key.Length != 16 && key.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 128 or 256 bit");
            if (iv.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");

            return DecryptInternal(buffer, key, iv);
        }
        /// <summary>
        /// Executes an AES decryption asynchronously.
        /// </summary>
        /// <param name="buffer">Ciphertext</param>
        /// <param name="key">AES key (128 or 256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        /// <returns></returns>
        public static Task<byte[]> DecryptAsync(byte[] buffer, byte[] key, byte[] iv)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (key == null) throw new ArgumentNullException("key");
            if (iv == null) throw new ArgumentNullException("iv");
            if (buffer.Length % 16 != 0) throw new ArgumentOutOfRangeException("buffer", "The input must have a valid block size");
            if (key.Length != 16 && key.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 128 or 256 bit");
            if (iv.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");

            return Task.Run(() => DecryptInternal(buffer, key, iv));
        }
        /// <summary>
        /// Generates a new 256 bit AES key.
        /// </summary>
        public static byte[] GenerateKey() => GenerateRandom(32);

        /// <summary>
        /// Generates a new 128 bit initialization vector.
        /// </summary>
        public static byte[] GenerateIV() => GenerateRandom(16);

        private static byte[] DecryptInternal(byte[] buffer, byte[] key, byte[] iv)
        {
            using (ICryptoTransform transform = aes.CreateDecryptor(key, iv))
                return ProcessData(buffer, transform);
        }

        private static byte[] EncryptInternal(byte[] buffer, byte[] key, byte[] iv)
        {
            using (ICryptoTransform transform = aes.CreateEncryptor(key, iv))
                return ProcessData(buffer, transform);
        }

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
    }
}