using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
#if WINDOWS_UWP
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
#endif

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
            byte[] ciphertext = null;
#if WINDOWS_UWP
            CryptographicKey ckey = AsymmetricKeyAlgorithmProvider.OpenAlgorithm("RSA_OAEP_SHA1").ImportPublicKey(ConvertToBlob(key).AsBuffer());
            DataReader dr = DataReader.FromBuffer(CryptographicEngine.Encrypt(ckey, plaintext.AsBuffer(), null));
            ciphertext = new byte[dr.UnconsumedBufferLength];
            dr.ReadBytes(ciphertext);
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
        /// Encrypts data using RSA with OAEP
        /// </summary>
        /// <param name="plaintext">data to encrypt</param>
        /// <param name="key">Public key (xmlstring)</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException"></exception>
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
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException"></exception>
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
            CryptographicKey ckey = AsymmetricKeyAlgorithmProvider.OpenAlgorithm("RSA_OAEP_SHA1").ImportPublicKey(ConvertToBlob(key).AsBuffer());
            DataReader dr = DataReader.FromBuffer(CryptographicEngine.Decrypt(ckey, plaintext.AsBuffer(), null));
            ciphertext = new byte[dr.UnconsumedBufferLength];
            dr.ReadBytes(ciphertext);
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
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
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
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
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

#if !WINDOWS_UWP
        /// <summary>
        /// Generates a random RSA keypair
        /// </summary>
        /// <returns></returns>
        [Obsolete("RSA.GenerateKeyPair() is deprecated, please use RSA.GenerateKeyPairXml or RSA.GenerateKeyPairBlob instead.", false)]
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
        /// Generates a random RSA keypair as a blob.
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateKeyPairBlob()
        {
            byte[] key;
#if WINDOWS_UWP
            CryptographicKey ckey = AsymmetricKeyAlgorithmProvider.OpenAlgorithm("RSA_OAEP_SHA1").CreateKeyPair(2048);
            DataReader dr = DataReader.FromBuffer(ckey.Export());
            uint count = dr.UnconsumedBufferLength;
            key = new byte[count];
            dr.ReadBytes(key);
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                key = ConvertToBlob(rsa.ToXmlString(true));
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
            CryptographicKey ckey = AsymmetricKeyAlgorithmProvider.OpenAlgorithm("RSA_OAEP_SHA1").CreateKeyPair(2048);
            DataReader dr = DataReader.FromBuffer(ckey.Export());
            uint count = dr.UnconsumedBufferLength;
            byte[] keyblob = new byte[count];
            dr.ReadBytes(keyblob);
            key = ConvertToXmlString(keyblob);
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                key = rsa.ToXmlString(true);
            }
#endif
            return key;
        }

        /// <summary>
        /// Extracts the parameters for a public key
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
            CryptographicKey ckey = AsymmetricKeyAlgorithmProvider.OpenAlgorithm("RSA_OAEP_SHA1").ImportKeyPair(ConvertToBlob(privateKey).AsBuffer());
            DataReader dr = DataReader.FromBuffer(ckey.ExportPublicKey());
            uint count = dr.UnconsumedBufferLength;
            byte[] keyblob = new byte[count];
            dr.ReadBytes(keyblob);
            key = ConvertToXmlString(keyblob);
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
        /// Extracts the parameters for a public key
        /// </summary>
        /// <param name="privateKey">Keypair (blob)</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException"></exception>
        /// <returns></returns>
        public static byte[] ExtractPublicKey(byte[] privateKey)
        {
            if (privateKey == null) throw new ArgumentNullException("PrivateKey must not be null");
            byte[] key;
#if WINDOWS_UWP
            CryptographicKey ckey = AsymmetricKeyAlgorithmProvider.OpenAlgorithm("RSA_OAEP_SHA1").ImportKeyPair(privateKey.AsBuffer());
            DataReader dr = DataReader.FromBuffer(ckey.ExportPublicKey());
            uint count = dr.UnconsumedBufferLength;
            key = new byte[count];
            dr.ReadBytes(key);
#else
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(ConvertToXmlString(privateKey));
                key = ConvertToBlob(rsa.ToXmlString(false));
            }
#endif
            return key;
        }
        public static string ConvertToXmlString(byte[] rsaKeyBlob)
        {
            // TODO: Implement conversion
            return "";
        }
        public static byte[] ConvertToBlob(string xmlKeyString)
        {
            // TODO: Implement conversion
            return new byte[0];
        }
    }
}