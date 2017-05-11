using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VSL.Crypt
{
    /// <summary>
    /// The simple implementation of the RSA algorithm in VSL
    /// </summary>
    public static class RSA
    {
        // © 2017 Daniel Lerch
        /// <summary>
        /// Encrypts one block using RSA with OAEP
        /// </summary>
        /// <param name="plaintext">Data to encrypt (max. 214 bytes)</param>
        /// <param name="key">Public key (xmlstring)</param>
        /// <returns></returns>
        public static byte[] EncryptBlock(byte[] plaintext, string key)
        {
            if (plaintext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            if (plaintext.Length > 214) throw new ArgumentOutOfRangeException("One block must measure 214 bytes");
            byte[] ciphertext = null;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(key);
                    ciphertext = rsa.Encrypt(plaintext, true);
                }
                catch (CryptographicException ex)
                {
                    throw new CryptographicException("Error during RSA encryption", ex);
                }
            }
            return ciphertext;
        }

        /// <summary>
        /// Encrypts data using RSA with OAEP
        /// </summary>
        /// <param name="plaintext">data to encrypt</param>
        /// <param name="key">Public key (xmlstring)</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] plaintext, string key)
        {
            if (plaintext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            byte[] ciphertext = new byte[0];
            foreach (byte[] block in Util.SplitBytes(plaintext, 214))
            {
                byte[] cipherblock = EncryptBlock(block, key);
                ciphertext = ciphertext.Concat(cipherblock).ToArray();
            }
            return ciphertext;
        }
        /// <summary>
        /// Encrypts data using RSA with OAEP asychronously
        /// </summary>
        /// <param name="plaintext">data to encrypt</param>
        /// <param name="key">Public key (xmlstring)</param>
        /// <returns></returns>
        public async static Task<byte[]> EncryptAsync(byte[] plaintext, string key)
        {
            if (plaintext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            byte[][] blocks = Util.SplitBytes(plaintext, 214);
            Task<byte[]>[] workers = new Task<byte[]>[blocks.Length]; //Works with nested arrays too!
            for (int i = 0; i < blocks.Length; i++)
            {
                byte[] block = blocks[i];
                workers[i] = Task.Run(() => EncryptBlock(block, key));
            }
            byte[] ciphertext = new byte[0];
            for (int i = 0; i < blocks.Length; i++)
            {
                ciphertext = ciphertext.Concat(await workers[i]).ToArray();
            }
            return ciphertext;
        }

        /// <summary>
        /// Decrypts one block using RSA with OAEP
        /// </summary>
        /// <param name="ciphertext">Data to decrypt (256 bytes)</param>
        /// <param name="key">Private key (xmlstring)</param>
        /// <returns></returns>
        public static byte[] DecryptBlock(byte[] ciphertext, string key)
        {
            if (ciphertext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            if (ciphertext.Length != 256) throw new ArgumentOutOfRangeException("One block must measure 256 bytes");
            byte[] plaintext = null;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(key);
                    plaintext = rsa.Decrypt(ciphertext, true);
                }
                catch (CryptographicException ex)
                {
                    throw new CryptographicException("Error during RSA decryption", ex);
                }
            }
            return plaintext;
        }

        /// <summary>
        /// Decrypts data using RSA with OAEP
        /// </summary>
        /// <param name="ciphertext">data to decrypt</param>
        /// <param name="key">Private key (xmlstring)</param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] ciphertext, string key)
        {
            if (ciphertext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            if (ciphertext.Length % 256 != 0) throw new ArgumentOutOfRangeException("The blocksize must be 256 bytes");
            byte[] plaintext = new byte[0];
            foreach (byte[] block in Util.SplitBytes(ciphertext, 256))
            {
                byte[] cipherblock = DecryptBlock(block, key);
                plaintext = plaintext.Concat(cipherblock).ToArray();
            }
            return plaintext;
        }
        /// <summary>
        /// Decrypts data using RSA with OAEP asychronously
        /// </summary>
        /// <param name="ciphertext">data to decrypt</param>
        /// <param name="key">Private key (xmlstring)</param>
        /// <returns></returns>
        public async static Task<byte[]> DecryptAsync(byte[] ciphertext, string key)
        {
            if (ciphertext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            if (ciphertext.Length % 256 != 0) throw new ArgumentOutOfRangeException("The blocksize must be 256 bytes");
            if (ciphertext.Length == 0) return ciphertext;
            byte[][] blocks = Util.SplitBytes(ciphertext, 256);
            Task<byte[]>[] workers = new Task<byte[]>[blocks.Length]; //Works with nested arrays too!
            for (int i = 0; i < blocks.Length; i++)
            {
                byte[] block = blocks[i];
                workers[i] = Task.Run(() => DecryptBlock(block, key));
            }
            byte[] plaintext = new byte[0];
            for (int i = 0; i < blocks.Length; i++)
            {
                plaintext = plaintext.Concat(await workers[i]).ToArray();
            }
            return plaintext;
        }

        /// <summary>
        /// Generates a random RSA keypair
        /// </summary>
        /// <returns></returns>
        public static string GenerateKeyPair()
        {
            string key;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                key = rsa.ToXmlString(true);
            }
            return key;
        }

        /// <summary>
        /// Extracts the parameters for a public key
        /// </summary>
        /// <param name="privateKey">Keypair (xmlstring)</param>
        /// <returns></returns>
        public static string ExtractPublicKey(string privateKey)
        {
            string key;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                key = rsa.ToXmlString(false);
            }
            return key;
        }
    }
}