using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Xml;

namespace VSL.Crypt
{
    /// <summary>
    /// The simple implementation of the RSA algorithm in VSL.
    /// </summary>
    public static class RsaStatic
    {
        // © 2017 Daniel Lerch
        #region Encrypt
        /// <summary>
        /// Encrypts one block using RSA with OAEP.
        /// </summary>
        /// <param name="plaintext">Data to encrypt (max. 214 bytes)</param>
        /// <param name="key">Public key (xmlstring)</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        /// <returns></returns>
        public static byte[] EncryptBlock(byte[] plaintext, string key)
        {
            if (plaintext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            if (plaintext.Length > 214) throw new ArgumentOutOfRangeException("One block must measure 214 bytes");
            using (var rsa = CreateProvider(key))
            {
                return EncryptInternal(rsa, plaintext);
            }
        }

        /// <summary>
        /// Encrypts data using RSA with OAEP.
        /// </summary>
        /// <param name="plaintext">Data to encrypt with any length.</param>
        /// <param name="key">Public key (xmlstring).</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException"></exception>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] plaintext, string key)
        {
            if (plaintext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            int blocks = Util.GetTotalSize(plaintext.Length, 214) / 214;
            byte[] ciphertext = new byte[blocks * 256];
            using (var rsa = CreateProvider(key))
            {
                Parallel.For(0, blocks - 1, (i) =>
                {
                    byte[] buf = EncryptInternal(rsa, Util.TakeBytes(plaintext, 214, i * 214));
                    Array.Copy(buf, 0, ciphertext, i * 256, 256);
                });
                int llen = plaintext.Length % 214;
                byte[] lbuf = EncryptInternal(rsa, Util.TakeBytes(plaintext, llen != 0 ? llen : 214, (blocks - 1) * 214));
                Array.Copy(lbuf, 0, ciphertext, (blocks - 1) * 256, 256);
            }
            return ciphertext;
        }

#if WINDOWS_UWP
        private static byte[] EncryptInternal(RSA rsa, byte[] rgb)
            => rsa.Encrypt(rgb, RSAEncryptionPadding.OaepSHA1);
#else
        private static byte[] EncryptInternal(RSACryptoServiceProvider rsa, byte[] rgb)
            => rsa.Encrypt(rgb, true);
#endif
        #endregion
        #region Decrypt
        /// <summary>
        /// Decrypts one block using RSA with OAEP
        /// </summary>
        /// <param name="ciphertext">Data to decrypt (256 bytes)</param>
        /// <param name="key">Private key (xmlstring)</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        /// <returns></returns>
        public static byte[] DecryptBlock(byte[] ciphertext, string key)
        {
            if (ciphertext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            if (ciphertext.Length != 256) throw new ArgumentOutOfRangeException("One block must measure 256 bytes");
            using (var rsa = CreateProvider(key))
                return DecryptInternal(rsa, ciphertext);
        }

        /// <summary>
        /// Decrypts data using RSA with OAEP
        /// </summary>
        /// <param name="ciphertext">data to decrypt</param>
        /// <param name="key">Private key (xmlstring)</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="CryptographicException"/>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] ciphertext, string key)
        {
            if (ciphertext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            if (ciphertext.Length % 256 != 0) throw new ArgumentOutOfRangeException("The blocksize must be 256 bytes");
            int blocks = ciphertext.Length / 256;
            byte[] tmp_plaintext = new byte[(blocks - 1) * 214];
            using (var rsa = CreateProvider(key))
            {
                Parallel.For(0, blocks - 1, (i) =>
                {
                    byte[] buf = DecryptInternal(rsa, Util.TakeBytes(ciphertext, 256, i * 256));
                    Array.Copy(buf, 0, tmp_plaintext, i * 214, buf.Length);
                });
                byte[] lbuf = DecryptInternal(rsa, Util.TakeBytes(ciphertext, 256, (blocks - 1) * 256));
                byte[] plaintext = new byte[(blocks - 1) * 214 + lbuf.Length];
                Array.Copy(tmp_plaintext, plaintext, (blocks - 1) * 214);
                Array.Copy(lbuf, 0, plaintext, (blocks - 1) * 214, lbuf.Length);
                return plaintext;
            }
        }
#if WINDOWS_UWP
        private static byte[] DecryptInternal(RSA rsa, byte[] rgb)
            => rsa.Decrypt(rgb, RSAEncryptionPadding.OaepSHA1);
#else
        private static byte[] DecryptInternal(RSACryptoServiceProvider rsa, byte[] rgb)
            => rsa.Decrypt(rgb, true);
#endif
        #endregion
        #region Generate Key
        /// <summary>
        /// Generates a random RSA keypair as a <see cref="RSAParameters"/> struct.
        /// </summary>
        /// <returns></returns>
        public static RSAParameters GenerateKeyPairParams()
        {
#if WINDOWS_UWP
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = 2048;
                return rsa.ExportParameters(true);
            }
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                return rsa.ExportParameters(true);
#endif
        }

        /// <summary>
        /// Generates a random RSA keypair in xml format.
        /// </summary>
        /// <returns></returns>
        public static string GenerateKeyPairXml()
        {
#if WINDOWS_UWP
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = 2048;
                return ToXmlString(rsa.ExportParameters(true));
            }
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                return rsa.ToXmlString(true);
#endif
        }

        /// <summary>
        /// Extracts the parameters for a public key.
        /// </summary>
        /// <param name="privateKey">Keypair (xmlstring)</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException"></exception>
        /// <returns></returns>
        public static string ExtractPublicKey(string privateKey)
        {
            if (privateKey == null) throw new ArgumentNullException("PrivateKey must not be null");
#if WINDOWS_UWP
            using (var rsa = System.Security.Cryptography.RSA.Create())
            {
                rsa.ImportParameters(GetParameters(privateKey));
                return ToXmlString(rsa.ExportParameters(false));
            }
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                return rsa.ToXmlString(false);
            }
#endif
        }

        /// <summary>
        /// Extracts the parameters for a public key.
        /// </summary>
        /// <param name="privateKey">Keypair (params)</param>
        /// <exception cref="CryptographicException"></exception>
        /// <returns></returns>
        public static RSAParameters ExtractPublicKey(RSAParameters privateKey)
        {
            using (var rsa = CreateProvider(privateKey))
                return rsa.ExportParameters(false);
        }
        #endregion
        #region Util
#if WINDOWS_UWP
        private static RSA CreateProvider(string key)
        {
            RSA rsa = RSA.Create();
            rsa.ImportParameters(GetParameters(key));
            return rsa;
        }
        private static RSA CreateProvider(RSAParameters key)
        {
            RSA rsa = RSA.Create();
            rsa.ImportParameters(key);
            return rsa;
        }
#else
        private static RSACryptoServiceProvider CreateProvider(string key)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
            return rsa;
        }
        private static RSACryptoServiceProvider CreateProvider(RSAParameters key)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(key);
            return rsa;
        }
#endif

        /// <summary>
        /// Converts a <see cref="RSAParameters"/> struct RSA key to the .NET XML format.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string ToXmlString(RSAParameters parameters)
        {
            StringBuilder result = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(result))
            {
                writer.WriteStartElement("RSAKeyValue");
                if (ValidByteArray(parameters.Modulus))
                    writer.WriteElementString("Modulus", Convert.ToBase64String(parameters.Modulus));
                if (ValidByteArray(parameters.Exponent))
                    writer.WriteElementString("Exponent", Convert.ToBase64String(parameters.Exponent));
                if (ValidByteArray(parameters.P))
                    writer.WriteElementString("P", Convert.ToBase64String(parameters.P));
                if (ValidByteArray(parameters.Q))
                    writer.WriteElementString("Q", Convert.ToBase64String(parameters.Q));
                if (ValidByteArray(parameters.DP))
                    writer.WriteElementString("DP", Convert.ToBase64String(parameters.DP));
                if (ValidByteArray(parameters.DQ))
                    writer.WriteElementString("DQ", Convert.ToBase64String(parameters.DQ));
                if (ValidByteArray(parameters.InverseQ))
                    writer.WriteElementString("InverseQ", Convert.ToBase64String(parameters.InverseQ));
                if (ValidByteArray(parameters.D))
                    writer.WriteElementString("D", Convert.ToBase64String(parameters.D));
                writer.WriteEndElement();
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets a <see cref="RSAParameters"/> struct from a .NET XML formatted key.
        /// </summary>
        /// <param name="xmlKeyString"></param>
        /// <returns></returns>
        public static RSAParameters GetParameters(string xmlKeyString)
        {
            RSAParameters parameters = new RSAParameters();
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlKeyString)))
            {
                string open = "";
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                        open = reader.Name;
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                        string val = reader.Value;
                        byte[] valB = Convert.FromBase64String(val);
                        switch (open)
                        {
                            case "Modulus":
                                parameters.Modulus = valB;
                                break;
                            case "Exponent":
                                parameters.Exponent = valB;
                                break;
                            case "P":
                                parameters.P = valB;
                                break;
                            case "Q":
                                parameters.Q = valB;
                                break;
                            case "DP":
                                parameters.DP = valB;
                                break;
                            case "DQ":
                                parameters.DQ = valB;
                                break;
                            case "InverseQ":
                                parameters.InverseQ = valB;
                                break;
                            case "D":
                                parameters.D = valB;
                                break;
                        }
                    }
                }
            }
            return parameters;
        }

        private static bool ValidByteArray(byte[] bt)
        {
            return bt != null && bt.Length > 0;
        }
        #endregion
    }
}