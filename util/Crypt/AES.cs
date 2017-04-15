using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VSL.Crypt
{
    /// <summary>
    /// The simple implementation of AES in VSL
    /// </summary>
    public static class AES
    {
        // © 2017 Daniel Lerch
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
            if (iv?.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] ciphertext = new byte[0];
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor();
                using (System.IO.MemoryStream msCiphertext = new System.IO.MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msCiphertext, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(b, 0, b.Length);
                        csEncrypt.Close();
                    }
                    ciphertext = msCiphertext.ToArray();
                }
            }
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
            if (iv?.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] ciphertext = new byte[0];
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor();
                using (System.IO.MemoryStream msCiphertext = new System.IO.MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msCiphertext, encryptor, CryptoStreamMode.Write))
                    {
                        await csEncrypt.WriteAsync(b, 0, b.Length);
                        csEncrypt.Close();
                    }
                    ciphertext = msCiphertext.ToArray();
                }
            }
            return ciphertext;
        }

        /// <summary>
        /// Executes an AES decryption
        /// </summary>
        /// <param name="b">Ciphertext</param>
        /// <param name="key">AES key (256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] b, byte[] key, byte[] iv = null)
        {
            if (key.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 256 bits");
            if (iv?.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bits");
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] plaintext = new byte[0];
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor();
                using (System.IO.MemoryStream msPlaintext = new System.IO.MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msPlaintext, decryptor, CryptoStreamMode.Write))
                    {
                        csDecrypt.Write(b, 0, b.Length);
                        csDecrypt.Close();
                    }
                    plaintext = msPlaintext.ToArray();
                }
            }
            return plaintext;
        }
        /// <summary>
        /// Executes an AES decryption asynchronously
        /// </summary>
        /// <param name="b">Ciphertext</param>
        /// <param name="key">AES key (256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <returns></returns>
        public async static Task<byte[]> DecryptAsync(byte[] b, byte[] key, byte[] iv = null)
        {
            if (key.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 256 bits");
            if (iv?.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bits");
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] plaintext = new byte[0];
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor();
                using (System.IO.MemoryStream msPlaintext = new System.IO.MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msPlaintext, decryptor, CryptoStreamMode.Write))
                    {
                        await csDecrypt.WriteAsync(b, 0, b.Length);
                        csDecrypt.Close();
                    }
                    plaintext = msPlaintext.ToArray();
                }
            }
            return plaintext;
        }

        /// <summary>
        /// Generates a new 256 bit AES key
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateKey()
        {
            byte[] key;
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                key = aes.Key;
            }
            return key;
        }

        /// <summary>
        /// Generates a new initialization vector
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateIV()
        {
            byte[] iv;
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.GenerateIV();
                iv = aes.IV;
            }
            return iv;
        }
    }
}