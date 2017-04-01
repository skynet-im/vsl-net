using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VSL.Crypt
{
    public static class RSA
    {
        // v9 © 2017 Daniel Lerch
        public async static Task<byte[]> Encrypt(byte[] b, string key)
        {
            if (b == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            try
            {
                byte[] ciphertext = new byte[0];
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(key);
                    foreach (byte[] block in Util.SplitBytes(b, 214))
                    {
                        byte[] cipherblock = await Task.Run(() => rsa.Encrypt(block, true));
                        ciphertext = ciphertext.Concat(cipherblock).ToArray();
                    }
                }
                return ciphertext;
            }
            catch (Exception ex)
            {
                throw new Exception("Error during encryption", ex);
            }
        }

        public async static Task<byte[]> Decrypt(byte[] b, string key)
        {
            if (b == null) throw new ArgumentNullException("Ciphertext must not be null");
            if (b.Length % 256 != 0) throw new ArgumentOutOfRangeException("The blocksize must be 256 bytes");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            try
            {
                byte[] plaintext = new byte[0];
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(key);
                    foreach (byte[] block in Util.SplitBytes(b, 256))
                    {
                        byte[] plainblock = await Task.Run(() => rsa.Decrypt(block, true));
                        plaintext = plaintext.Concat(plainblock).ToArray();
                    }
                }
                return plaintext;
            }
            catch (Exception ex)
            {
                throw new Exception("Error during encryption", ex);
            }
        }

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