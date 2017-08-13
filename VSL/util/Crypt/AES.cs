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
    public class AES : IDisposable
    {
        // © 2017 Daniel Lerch
        private AesCryptoServiceProvider csp;
        private ICryptoTransform encryptor;
        private ICryptoTransform decryptor;
        /// <summary>
        /// Initializes a new instance of the AES class. This should be more efficient than the static methods for multiple operations with the same key.
        /// </summary>
        /// <param name="key">AES key (256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public AES(byte[] key, byte[] iv = null)
        {
            if (key.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 256 bit");
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            if (iv.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");
            csp = new AesCryptoServiceProvider()
            {
                Key = key,
                IV = iv
            };
            encryptor = csp.CreateEncryptor();
            decryptor = csp.CreateDecryptor();
        }
        /// <summary>
        /// Sets the key for the next operations.
        /// </summary>
        public byte[] Key
        {
            set
            {
                if (value.Length != 32) throw new ArgumentOutOfRangeException("The key must have a length of 256 bit");
                csp.Key = value;
                encryptor = csp.CreateEncryptor();
                decryptor = csp.CreateDecryptor();
            }
        }
        /// <summary>
        /// Sets the initialization vector for the next operations.
        /// </summary>
        public byte[] IV
        {
            set
            {
                if (value == null) value = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                if (value.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");
                csp.IV = value;
                encryptor = csp.CreateEncryptor();
                decryptor = csp.CreateDecryptor();
            }
        }

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
            if (iv == null) iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            if (iv.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bits");
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
        /// Generates a new initialization vector.
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
        /// <summary>
        /// Generates a new initialization vector.
        /// </summary>
        /// <param name="set">Specifies whether the new IV should be set for the next operations.</param>
        /// <returns></returns>
        public byte[] GenerateIV(bool set)
        {
            if (set)
            {
                csp.GenerateIV();
                IV = csp.IV;
                return csp.IV;
            }
            else
            {
                byte[] oldIV = csp.IV;
                csp.GenerateIV();
                byte[] newIV = csp.IV;
                csp.IV = oldIV;
                return newIV;
            }
        }

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
                    csp.Dispose();
                    encryptor.Dispose();
                    decryptor.Dispose();
                }

                // -TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // -TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // -TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AES() {
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
            // -TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}