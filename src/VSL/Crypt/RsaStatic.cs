using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using VSL.BinaryTools;

namespace VSL.Crypt
{
    /// <summary>
    /// The simple implementation of the RSA algorithm in VSL.
    /// </summary>
    public static class RsaStatic
    {
        // © 2017-2018 Daniel Lerch
        #region Encrypt
        /// <summary>
        /// Encrypts one block using RSA with OAEP.
        /// </summary>
        /// <param name="plaintext">Data to encrypt (max. 214 bytes).</param>
        /// <param name="key">Public key as xml string.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        public static byte[] EncryptBlock(byte[] plaintext, string key)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (plaintext.Length > 214) throw new ArgumentOutOfRangeException(nameof(plaintext), plaintext.Length, "One block must measure 214 bytes");

            return EncryptBlock(plaintext, new RSAParameters().ImportXmlKey(key));
        }

        /// <summary>
        /// Encrypts one block using RSA with OAEP.
        /// </summary>
        /// <param name="plaintext">Data to encrypt (max. 214 bytes).</param>
        /// <param name="key"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        public static byte[] EncryptBlock(byte[] plaintext, RSAParameters key)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));
            if (plaintext.Length > 214) throw new ArgumentOutOfRangeException(nameof(plaintext), plaintext.Length, "One block must measure 214 bytes");
            using (var rsa = CreateProvider(key))
            {
                return EncryptInternal(rsa, plaintext);
            }
        }

        /// <summary>
        /// Encrypts data using RSA with OAEP.
        /// </summary>
        /// <param name="plaintext">Data to encrypt with any length.</param>
        /// <param name="key">Public key as xml string.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException"></exception>
        public static byte[] Encrypt(byte[] plaintext, string key)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            return Encrypt(plaintext, new RSAParameters().ImportXmlKey(key));
        }

        /// <summary>
        /// Encrypts data using RSA with OAEP.
        /// </summary>
        /// <param name="plaintext">Data to encrypt with any length.</param>
        /// <param name="key"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException"></exception>
        public static byte[] Encrypt(byte[] plaintext, RSAParameters key)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));

            int blocks = Util.GetTotalSize(plaintext.Length, 214) / 214;
            byte[] ciphertext = new byte[blocks * 256];
            using (RSA rsa = CreateProvider(key))
            {
                Parallel.For(0, blocks - 1, (i) =>
                {
                    byte[] buf = EncryptInternal(rsa, plaintext.TakeAt(i * 214, 214));
                    Array.Copy(buf, 0, ciphertext, i * 256, 256);
                });
                int lidx = (blocks - 1) * 214;
                int llen = plaintext.Length - lidx;
                byte[] lbuf = EncryptInternal(rsa, plaintext.TakeAt(lidx, llen));
                Array.Copy(lbuf, 0, ciphertext, (blocks - 1) * 256, 256);
            }
            return ciphertext;
        }

        private static byte[] EncryptInternal(RSA rsa, byte[] rgb)
        {
            return rsa.Encrypt(rgb, RSAEncryptionPadding.OaepSHA1);
        }

        #endregion
        #region Decrypt
        /// <summary>
        /// Decrypts one block using RSA with OAEP.
        /// </summary>
        /// <param name="ciphertext">Data to decrypt as a single 256 byte block.</param>
        /// <param name="key">Private key as xml string.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        public static byte[] DecryptBlock(byte[] ciphertext, string key)
        {
            if (ciphertext == null) throw new ArgumentNullException(nameof(ciphertext));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(ciphertext));
            if (ciphertext.Length != 256) throw new ArgumentOutOfRangeException(nameof(ciphertext), ciphertext.Length, "One block must measure 256 bytes");

            return DecryptBlock(ciphertext, new RSAParameters().ImportXmlKey(key));
        }

        /// <summary>
        /// Decrypts one block using RSA with OAEP.
        /// </summary>
        /// <param name="ciphertext">Data to decrypt as a single 256 byte block.</param>
        /// <param name="key"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        public static byte[] DecryptBlock(byte[] ciphertext, RSAParameters key)
        {
            if (ciphertext == null) throw new ArgumentNullException(nameof(ciphertext));
            if (ciphertext.Length != 256) throw new ArgumentOutOfRangeException(nameof(ciphertext), ciphertext.Length, "One block must measure 256 bytes");

            using (RSA rsa = CreateProvider(key))
            {
                return DecryptInternal(rsa, ciphertext);
            }
        }

        /// <summary>
        /// Decrypts data using RSA with OAEP.
        /// </summary>
        /// <param name="ciphertext">Data to decrypt in multiple 256 byte blocks.</param>
        /// <param name="key">Private key as xml string.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        public static byte[] Decrypt(byte[] ciphertext, string key)
        {
            if (ciphertext == null) throw new ArgumentNullException(nameof(ciphertext));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(ciphertext));
            if (ciphertext.Length % 256 != 0) throw new ArgumentOutOfRangeException(nameof(ciphertext), ciphertext.Length % 256, "The blocksize must be 256 bytes");

            return Decrypt(ciphertext, new RSAParameters().ImportXmlKey(key));
        }

        /// <summary>
        /// Decrypts data using RSA with OAEP.
        /// </summary>
        /// <param name="ciphertext">Data to decrypt in multiple 256 byte blocks.</param>
        /// <param name="key"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        public static byte[] Decrypt(byte[] ciphertext, RSAParameters key)
        {
            if (ciphertext == null) throw new ArgumentNullException(nameof(ciphertext));
            if (ciphertext.Length % 256 != 0) throw new ArgumentOutOfRangeException(nameof(ciphertext), ciphertext.Length % 256, "The blocksize must be 256 bytes");

            int blocks = ciphertext.Length / 256;
            byte[] tmp_plaintext = new byte[(blocks - 1) * 214];
            using (var rsa = CreateProvider(key))
            {
                Parallel.For(0, blocks - 1, (i) =>
                {
                    byte[] buf = DecryptInternal(rsa, ciphertext.TakeAt(i * 256, 256));
                    Array.Copy(buf, 0, tmp_plaintext, i * 214, buf.Length);
                });
                byte[] lbuf = DecryptInternal(rsa, ciphertext.TakeAt((blocks - 1) * 256, 256));
                byte[] plaintext = new byte[(blocks - 1) * 214 + lbuf.Length];
                Array.Copy(tmp_plaintext, plaintext, (blocks - 1) * 214);
                Array.Copy(lbuf, 0, plaintext, (blocks - 1) * 214, lbuf.Length);
                return plaintext;
            }
        }

        private static byte[] DecryptInternal(RSA rsa, byte[] rgb)
        {
            return rsa.Decrypt(rgb, RSAEncryptionPadding.OaepSHA1);
        }

        #endregion
        #region Generate Key
        /// <summary>
        /// Generates a random RSA keypair as a <see cref="RSAParameters"/> struct.
        /// </summary>
        public static RSAParameters GenerateKeyPairParams()
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = 2048;
                return rsa.ExportParameters(true);
            }
        }

        /// <summary>
        /// Generates a random RSA keypair in xml format.
        /// </summary>
        public static string GenerateKeyPairXml()
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = 2048;
                return rsa.ExportParameters(true).ExportXmlKey();
            }
        }

        /// <summary>
        /// Extracts the parameters for a public key.
        /// </summary>
        /// <param name="privateKey">Keypair (xmlstring)</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException"></exception>
        public static string ExtractPublicKey(string privateKey)
        {
            if (privateKey == null) throw new ArgumentNullException(nameof(privateKey));

            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(new RSAParameters().ImportXmlKey(privateKey));
                return rsa.ExportParameters(false).ExportXmlKey();
            }
        }

        /// <summary>
        /// Extracts the parameters for a public key.
        /// </summary>
        /// <param name="privateKey">Keypair (params)</param>
        /// <exception cref="CryptographicException"></exception>
        public static RSAParameters ExtractPublicKey(RSAParameters privateKey)
        {
            using (RSA rsa = CreateProvider(privateKey))
            {
                return rsa.ExportParameters(false);
            }
        }
        #endregion
        #region Util
        private static RSA CreateProvider(RSAParameters key)
        {
            RSA rsa = RSA.Create();
            rsa.ImportParameters(key);
            return rsa;
        }
        #endregion
    }
}