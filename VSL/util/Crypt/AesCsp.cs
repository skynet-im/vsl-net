using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VSL.Crypt
{
    /// <summary>
    /// The simple instance implementation of an AES crypto service provider.
    /// </summary>
    public class AesCsp : IDisposable
    {
        // © 2017 Daniel Lerch
        private AesCryptoServiceProvider csp;
        private ICryptoTransform encryptor;
        private ICryptoTransform decryptor;
        /// <summary>
        /// Initializes a new instance of the AesCsp class which is more efficient than the static <see cref="AES"/> class for multiple operations with the same key.
        /// </summary>
        /// <param name="key">AES key (256 bit)</param>
        /// <param name="iv">Optional initialization vector (128 bit)</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public AesCsp(byte[] key, byte[] iv = null)
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
            _encryptIV = iv;
            _decryptIV = iv;
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
                csp.IV = _encryptIV;
                encryptor = csp.CreateEncryptor();
                csp.IV = _decryptIV;
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
                _encryptIV = value;
                _decryptIV = value;
                encryptor = csp.CreateEncryptor();
                decryptor = csp.CreateDecryptor();
            }
        }
        private byte[] _encryptIV;
        /// <summary>
        /// Sets the initialization vector for the next encryptions.
        /// </summary>
        public byte[] EncryptIV
        {
            set
            {
                if (value == null) value = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                if (value.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");
                csp.IV = value;
                _encryptIV = value;
                encryptor = csp.CreateEncryptor();
            }
        }
        private byte[] _decryptIV;
        /// <summary>
        /// Sets the initialization vector for the next decryptions.
        /// </summary>
        public byte[] DecryptIV
        {
            set
            {
                if (value == null) value = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                if (value.Length != 16) throw new ArgumentOutOfRangeException("The initialization vector must have a length of 128 bit");
                csp.IV = value;
                _decryptIV = value;
                decryptor = csp.CreateDecryptor();
            }
        }

        /// <summary>
        /// Executes an AES encryption.
        /// </summary>
        /// <param name="buf">The plaintext to encrypt.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="CryptographicException"/>
        /// <returns></returns>
        public byte[] Encrypt(byte[] buf)
        {
            if (buf == null) throw new ArgumentNullException("buf");
            byte[] ciphertext = new byte[0];
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                System.IO.MemoryStream msCiphertext = new System.IO.MemoryStream();
                CryptoStream csEncrypt = new CryptoStream(msCiphertext, encryptor, CryptoStreamMode.Write);
                csEncrypt.Write(buf, 0, buf.Length);
                csEncrypt.Close();
                ciphertext = msCiphertext.ToArray();
            }
            return ciphertext;
        }
        /// <summary>
        /// Executes an AES encryption asychronously.
        /// </summary>
        /// <param name="buf">The plaintext to encrypt.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="CryptographicException"/>
        /// <returns></returns>
        public async Task<byte[]> EncryptAsync(byte[] buf)
        {
            if (buf == null) throw new ArgumentNullException("buf");
            byte[] ciphertext = new byte[0];
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                using (System.IO.MemoryStream msCiphertext = new System.IO.MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msCiphertext, encryptor, CryptoStreamMode.Write))
                    {
                        await csEncrypt.WriteAsync(buf, 0, buf.Length);
                        csEncrypt.Close();
                    }
                    ciphertext = msCiphertext.ToArray();
                }
            }
            return ciphertext;
        }

        /// <summary>
        /// Executes an AES decryption.
        /// </summary>
        /// <param name="buf">The ciphertext to decrypt.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        /// <returns></returns>
        public byte[] Decrypt(byte[] buf)
        {
            if (buf == null) throw new ArgumentNullException("buf");
            if (buf.Length % 16 != 0) throw new ArgumentOutOfRangeException("buf", "The blocksize must be 16 bytes.");
            byte[] plaintext = new byte[0];
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                System.IO.MemoryStream msPlaintext = new System.IO.MemoryStream();
                CryptoStream csDecrypt = new CryptoStream(msPlaintext, decryptor, CryptoStreamMode.Write);
                csDecrypt.Write(buf, 0, buf.Length);
                csDecrypt.Close();
                plaintext = msPlaintext.ToArray();
            }
            return plaintext;
        }
        /// <summary>
        /// Executes an AES decryption asynchronously
        /// </summary>
        /// <param name="buf">Ciphertext</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        /// <returns></returns>
        public async Task<byte[]> DecryptAsync(byte[] buf)
        {
            if (buf == null) throw new ArgumentNullException("buf");
            if (buf.Length % 16 != 0) throw new ArgumentOutOfRangeException("buf", "The blocksize must be 16 bytes.");
            byte[] plaintext = new byte[0];
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                using (System.IO.MemoryStream msPlaintext = new System.IO.MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msPlaintext, decryptor, CryptoStreamMode.Write))
                    {
                        await csDecrypt.WriteAsync(buf, 0, buf.Length);
                        csDecrypt.Close();
                    }
                    plaintext = msPlaintext.ToArray();
                }
            }
            return plaintext;
        }
        /// <summary>
        /// Generates a new key.
        /// </summary>
        /// <param name="set">Specifies whether the new key should be set for the next operations.</param>
        /// <returns></returns>
        public byte[] GenerateKey(bool set)
        {
            if (set)
            {
                csp.GenerateKey();
                Key = csp.Key;
                return csp.Key;
            }
            else
            {
                byte[] oldKey = csp.Key;
                csp.GenerateKey();
                byte[] newKey = csp.Key;
                csp.Key = oldKey;
                return newKey;
            }
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