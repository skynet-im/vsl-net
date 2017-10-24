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
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        /// <returns></returns>
        public static byte[] EncryptBlock(byte[] plaintext, string key)
        {
            if (plaintext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            if (plaintext.Length > 214) throw new ArgumentOutOfRangeException("One block must measure 214 bytes");
            byte[] ciphertext;
#if WINDOWS_UWP
            using (var rsa = System.Security.Cryptography.RSA.Create())
            {
                rsa.ImportParameters(GetParameters(key));
                ciphertext = rsa.Encrypt(plaintext, RSAEncryptionPadding.OaepSHA1);
            }          
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(key);
                ciphertext = rsa.Encrypt(plaintext, true);
            }
#endif
            return ciphertext;
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
            int blocks = Convert.ToInt32(Math.Ceiling(plaintext.Length / Convert.ToSingle(214)));
            byte[] ciphertext = new byte[blocks * 256];
            Parallel.For(0, blocks - 1, (i) =>
            {
                byte[] buf = EncryptBlock(Util.TakeBytes(plaintext, 214, i * 214), key);
                Array.Copy(buf, 0, ciphertext, i * 256, 256);
            });
            int mod = blocks % 214;
            byte[] lbuf = EncryptBlock(Util.TakeBytes(plaintext, mod != 0 ? mod : 214, (blocks - 1) * 214), key);
            Array.Copy(lbuf, 0, ciphertext, (blocks - 1) * 256, 256);
            return ciphertext;
        }
        /// <summary>
        /// Encrypts data using RSA with OAEP asychronously
        /// </summary>
        /// <param name="plaintext">data to encrypt</param>
        /// <param name="key">Public key (xmlstring)</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException"></exception>
        /// <returns></returns>
        [Obsolete("RSA.EncryptAsync is deprecated, please use Task.Run with RSA.Encrypt instead.", false)]
        // TODO: Add error in v1.1.19.0
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
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        /// <returns></returns>
        public static byte[] DecryptBlock(byte[] ciphertext, string key)
        {
            if (ciphertext == null) throw new ArgumentNullException("Plaintext must not be null");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key must not be null");
            if (ciphertext.Length != 256) throw new ArgumentOutOfRangeException("One block must measure 256 bytes");
            byte[] plaintext = null;
#if WINDOWS_UWP
            using (var rsa = System.Security.Cryptography.RSA.Create())
            {
                rsa.ImportParameters(GetParameters(key));
                plaintext = rsa.Decrypt(ciphertext, RSAEncryptionPadding.OaepSHA1);
            }
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(key);
                plaintext = rsa.Decrypt(ciphertext, true);
            }
#endif
            return plaintext;
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
            Parallel.For(0, blocks - 1, (i) =>
            {
                byte[] buf = DecryptBlock(Util.TakeBytes(ciphertext, 256, i * 256), key);
                Array.Copy(buf, 0, tmp_plaintext, i * 214, buf.Length);
            });
            byte[] lbuf = DecryptBlock(Util.TakeBytes(ciphertext, 256, (blocks - 1) * 256), key);
            byte[] plaintext = new byte[(blocks - 1) * 214 + lbuf.Length];
            Array.Copy(tmp_plaintext, plaintext, (blocks - 1) * 214);
            Array.Copy(lbuf, 0, plaintext, (blocks - 1) * 214, lbuf.Length);
            return plaintext;
        }
        /// <summary>
        /// Decrypts data using RSA with OAEP asychronously
        /// </summary>
        /// <param name="ciphertext">data to decrypt</param>
        /// <param name="key">Private key (xmlstring)</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        /// <returns></returns>
        [Obsolete("RSA.DecryptAsync is deprecated, please use Task.Run with RSA.Decrypt instead.", false)]
        // TODO: Add error in v1.1.19.0
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

#if !WINDOWS_UWP
        /// <summary>
        /// Generates a random RSA keypair
        /// </summary>
        /// <returns></returns>
        [Obsolete("RSA.GenerateKeyPair() is deprecated, please use RSA.GenerateKeyPairXml or RSA.GenerateKeyPairParams instead.", false)]
        // TODO: Add error in v1.1.19.0
        public static string GenerateKeyPair()
        {
            string key;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                key = rsa.ToXmlString(true);
            }
            return key;
        }
#endif
        /// <summary>
        /// Generates a random RSA keypair as a <see cref="RSAParameters"/> struct.
        /// </summary>
        /// <returns></returns>
        public static RSAParameters GenerateKeyPairParams()
        {
            RSAParameters key;
#if WINDOWS_UWP
            using (var rsa = System.Security.Cryptography.RSA.Create())
            {
                rsa.KeySize = 2048;
                key = rsa.ExportParameters(true);
            }
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                key = rsa.ExportParameters(true);
            }
#endif
            return key;
        }

        /// <summary>
        /// Generates a random RSA keypair in xml format.
        /// </summary>
        /// <returns></returns>
        public static string GenerateKeyPairXml()
        {
            string key;
#if WINDOWS_UWP
            using (var rsa = System.Security.Cryptography.RSA.Create())
            {
                rsa.KeySize = 2048;
                key = ToXmlString(rsa.ExportParameters(true));
            }
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                key = rsa.ToXmlString(true);
            }
#endif
            return key;
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
            string key;
#if WINDOWS_UWP
            using (var rsa = System.Security.Cryptography.RSA.Create())
            {
                rsa.ImportParameters(GetParameters(privateKey));
                key = ToXmlString(rsa.ExportParameters(false));
            }
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                key = rsa.ToXmlString(false);
            }
#endif
            return key;
        }

        /// <summary>
        /// Extracts the parameters for a public key.
        /// </summary>
        /// <param name="privateKey">Keypair (params)</param>
        /// <exception cref="CryptographicException"></exception>
        /// <returns></returns>
        public static RSAParameters ExtractPublicKey(RSAParameters privateKey)
        {
            RSAParameters key;
#if WINDOWS_UWP
            using (var rsa = System.Security.Cryptography.RSA.Create())
            {
                rsa.ImportParameters(privateKey);
                key = rsa.ExportParameters(false);
            }
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(ToXmlString(privateKey));
                key = GetParameters(rsa.ToXmlString(false));
            }
#endif
            return key;
        }

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
    }
}