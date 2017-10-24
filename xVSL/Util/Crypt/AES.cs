using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
#endif

namespace VSL.Crypt
{
    /// <summary>
    /// The simple implementation of AES in VSL
    /// </summary>
    public static class AES
    {
        /// <summary>
        /// Executes an AES encryption
        /// </summary>
        /// <param name="b">Plaintext</param>
        /// <param name="key">AES key (256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] b, byte[] key, byte[] iv = null)
        {
            if (key.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 256 bit");
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            if (iv.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");
            byte[] ciphertext = new byte[0];
#if WINDOWS_UWP
            IBuffer bkey = CryptographicBuffer.CreateFromByteArray(key);
            CryptographicKey ckey = SymmetricKeyAlgorithmProvider.OpenAlgorithm("AES_CBC_PKCS7").CreateSymmetricKey(bkey);
            IBuffer data = CryptographicBuffer.CreateFromByteArray(b);
            IBuffer biv = CryptographicBuffer.CreateFromByteArray(iv);
            IBuffer bciphertext = CryptographicEngine.Encrypt(ckey, data, biv);
            CryptographicBuffer.CopyToByteArray(bciphertext, out ciphertext);
#else
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor();
                System.IO.MemoryStream msCiphertext = new System.IO.MemoryStream();
                CryptoStream csEncrypt = new CryptoStream(msCiphertext, encryptor, CryptoStreamMode.Write);
                csEncrypt.Write(b, 0, b.Length);
                csEncrypt.Close();
                ciphertext = msCiphertext.ToArray();
            }
#endif
            return ciphertext;
        }

        /// <summary>
        /// Executes an AES encryption asychronously
        /// </summary>
        /// <param name="b">Plaintext</param>
        /// <param name="key">AES key (256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <returns></returns>
        public async static Task<byte[]> EncryptAsync(byte[] b, byte[] key, byte[] iv = null)
        {
            if (key.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 256 bit");
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            if (iv.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");
            byte[] ciphertext = new byte[0];
#if WINDOWS_UWP
            await Task.Run(() =>
            {
                IBuffer bkey = CryptographicBuffer.CreateFromByteArray(key);
                CryptographicKey ckey = SymmetricKeyAlgorithmProvider.OpenAlgorithm("AES_CBC_PKCS7").CreateSymmetricKey(bkey);
                IBuffer data = CryptographicBuffer.CreateFromByteArray(b);
                IBuffer biv = CryptographicBuffer.CreateFromByteArray(iv);
                IBuffer bciphertext = CryptographicEngine.Encrypt(ckey, data, biv);
                CryptographicBuffer.CopyToByteArray(bciphertext, out ciphertext);
            });
#else
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor();
                System.IO.MemoryStream msCiphertext = new System.IO.MemoryStream();
                CryptoStream csEncrypt = new CryptoStream(msCiphertext, encryptor, CryptoStreamMode.Write);
                await csEncrypt.WriteAsync(b, 0, b.Length);
                csEncrypt.Close();
                ciphertext = msCiphertext.ToArray();
            }
#endif
            return ciphertext;
        }

        /// <summary>
        /// Executes an AES decryption.
        /// </summary>
        /// <param name="b">Ciphertext</param>
        /// <param name="key">AES key (256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] b, byte[] key, byte[] iv = null)
        {
            if (b == null) throw new ArgumentNullException("b");
            if (key == null) throw new ArgumentNullException("key");
            if (key.Length != 32) throw new ArgumentOutOfRangeException("key", "The key must have a length of 256 bits");
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            if (iv.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bits");
            byte[] plaintext = new byte[0];
#if WINDOWS_UWP
            IBuffer bkey = CryptographicBuffer.CreateFromByteArray(key);
            CryptographicKey ckey = SymmetricKeyAlgorithmProvider.OpenAlgorithm("AES_CBC_PKCS7").CreateSymmetricKey(bkey);
            IBuffer data = CryptographicBuffer.CreateFromByteArray(b);
            IBuffer biv = CryptographicBuffer.CreateFromByteArray(iv);
            IBuffer bplaintext = CryptographicEngine.Decrypt(ckey, data, biv);
            CryptographicBuffer.CopyToByteArray(bplaintext, out plaintext);
#else
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor();
                System.IO.MemoryStream msPlaintext = new System.IO.MemoryStream();
                CryptoStream csDecrypt = new CryptoStream(msPlaintext, decryptor, CryptoStreamMode.Write);
                csDecrypt.Write(b, 0, b.Length);
                csDecrypt.Close();
                plaintext = msPlaintext.ToArray();
            }
#endif
            return plaintext;
        }
        /// <summary>
        /// Executes an AES decryption asynchronously.
        /// </summary>
        /// <param name="b">Ciphertext</param>
        /// <param name="key">AES key (256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        /// <returns></returns>
        public static async Task<byte[]> DecryptAsync(byte[] b, byte[] key, byte[] iv = null)
        {
            if (b == null) throw new ArgumentNullException("b");
            if (key == null) throw new ArgumentNullException("key");
            if (key.Length != 32) throw new ArgumentOutOfRangeException("key", "The key must have a length of 256 bits");
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            if (iv.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bits");
            byte[] plaintext = new byte[0];
#if WINDOWS_UWP
            IBuffer bkey = CryptographicBuffer.CreateFromByteArray(key);
            CryptographicKey ckey = SymmetricKeyAlgorithmProvider.OpenAlgorithm("AES_CBC_PKCS7").CreateSymmetricKey(bkey);
            IBuffer data = CryptographicBuffer.CreateFromByteArray(b);
            IBuffer biv = CryptographicBuffer.CreateFromByteArray(iv);
            IBuffer bplaintext = await CryptographicEngine.DecryptAsync(ckey, data, biv);
            CryptographicBuffer.CopyToByteArray(bplaintext, out plaintext);
#else
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor();
                System.IO.MemoryStream msPlaintext = new System.IO.MemoryStream();
                CryptoStream csDecrypt = new CryptoStream(msPlaintext, decryptor, CryptoStreamMode.Write);
                await csDecrypt.WriteAsync(b, 0, b.Length);
                csDecrypt.Close();
                plaintext = msPlaintext.ToArray();
            }
#endif
            return plaintext;
        }
        /// <summary>
        /// Generates a new 256 bit AES key
        /// </summary>
        public static byte[] GenerateKey()
        {
            byte[] key;
#if WINDOWS_UWP
            key = new byte[32];
            DataReader.FromBuffer(CryptographicBuffer.GenerateRandom(32)).ReadBytes(key);
#else
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                key = aes.Key;
            }
#endif
            return key;
        }

        /// <summary>
        /// Generates a new initialization vector.
        /// </summary>
        public static byte[] GenerateIV()
        {
            byte[] iv;
#if WINDOWS_UWP
            iv = new byte[16];
            DataReader.FromBuffer(CryptographicBuffer.GenerateRandom(16)).ReadBytes(iv);
#else
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.GenerateIV();
                iv = aes.IV;
            }
#endif
            return iv;
        }
    }
}