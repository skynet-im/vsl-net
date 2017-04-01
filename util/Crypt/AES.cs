using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VSL.Crypt
{
    public static class AES
    {
        // v9 © 2017 Daniel Lerch
        /// <summary>
        /// Executes an AES encryption
        /// </summary>
        /// <param name="b">Plaintext</param>
        /// <param name="key">AES key (256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <returns></returns>
        public async static Task<byte[]> Encrypt(byte[] b, byte[] key, byte[] iv = null)
        {
            if (key.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 256 bit");
            if (iv?.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] encrypted = new byte[0];
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor();
                using (System.IO.MemoryStream msEncrypt = new System.IO.MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        await csEncrypt.WriteAsync(b, 0, b.Length);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
            return encrypted;
        }
        /// <summary>
        /// Executes an AES decryption
        /// </summary>
        /// <param name="b">Ciphertext</param>
        /// <param name="key">AES key (256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <returns></returns>
        public async static Task<byte[]> Decrypt(byte[] b, byte[] key, byte[] iv = null)
        {
            if (key.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 256 bits");
            if (iv?.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bits");
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] decrypted = new byte[0];
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor();
                using (System.IO.MemoryStream msCipher = new System.IO.MemoryStream(b))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msCipher, decryptor, CryptoStreamMode.Read))
                    {
                        using (System.IO.MemoryStream msDecrypt = new System.IO.MemoryStream())
                        {
                            await csDecrypt.CopyToAsync(msDecrypt);
                            decrypted = msDecrypt.ToArray();
                        }
                    }
                }
            }
            return decrypted;
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