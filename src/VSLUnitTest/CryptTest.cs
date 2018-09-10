using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using VSL;
using VSL.BinaryTools;
using VSL.Crypt;

namespace VSLUnitTest
{
    [TestClass]
    public class CryptTest
    {
        [TestMethod]
        public void TestRsa()
        {
            string @private = RsaStatic.GenerateKeyPairXml();
            string @public = RsaStatic.ExtractPublicKey(@private);
            Assert.AreNotEqual(@private, @public, true);
            Assert.AreNotEqual(@private.Length, @public.Length);

            Random random = new Random();
            byte[] plaintext = new byte[184];
            random.NextBytes(plaintext);

            byte[] ciphertext = RsaStatic.EncryptBlock(plaintext, @public);
            Assert.AreEqual(ciphertext.Length, 256);

            byte[] decrypted = RsaStatic.DecryptBlock(ciphertext, @private);
            CollectionAssert.AreEqual(plaintext, decrypted);

            // Test multiple blocks
            plaintext = new byte[3456];
            random.NextBytes(plaintext);

            ciphertext = RsaStatic.Encrypt(plaintext, @public);

            decrypted = RsaStatic.Decrypt(ciphertext, @private);
            CollectionAssert.AreEqual(plaintext, decrypted);
        }

        [TestMethod]
        public void TestAes()
        {
            byte[] key = AesStatic.GenerateKey();
            Assert.IsNotNull(key);
            byte[] iv = AesStatic.GenerateIV();
            Assert.IsNotNull(iv);

            Random random = new Random();
            byte[] plaintext = new byte[45674];
            random.NextBytes(plaintext);

            byte[] ciphertext = AesStatic.Encrypt(plaintext, key, iv);
            Assert.IsNotNull(ciphertext);
            Assert.AreEqual(ciphertext.Length, Util.GetTotalSize(plaintext.Length, 16));

            byte[] decrypted = AesStatic.Decrypt(ciphertext, key, iv);
            Assert.IsNotNull(decrypted);
            CollectionAssert.AreEqual(plaintext, decrypted);
        }

        [TestMethod]
        public void TestAesHmac()
        {
            byte[] hmac = AesStatic.GenerateKey();
            byte[] key = AesStatic.GenerateKey();
            byte[] iv = AesStatic.GenerateIV();

            Random random = new Random();
            byte[] plaintext = new byte[69854];
            random.NextBytes(plaintext);

            PacketBuffer ciphertext = PacketBuffer.CreateDynamic();
            AesStatic.EncryptWithHmac(plaintext, ciphertext, false, hmac, key);
            ciphertext.Position = 0;
            byte[] result = AesStatic.DecryptWithHmac(ciphertext, -1, hmac, key);

            CollectionAssert.AreEqual(plaintext, result);
        }

        [TestMethod]
        public void TestSha()
        {
            byte[] sha1Utf8Empty = Util.GetBytes(Constants.Sha1Utf8Empty);
            byte[] sha256Utf8Empty = Util.GetBytes(Constants.Sha256Utf8Empty);

            byte[] sha1 = Hash.SHA1("", Encoding.UTF8);
            CollectionAssert.AreEqual(sha1Utf8Empty, sha1, "Empty SHA-1 hash invalid.");

            byte[] sha256 = Hash.SHA256("", Encoding.UTF8);
            CollectionAssert.AreEqual(sha256Utf8Empty, sha256, "Empty SHA-256 hash invalid.");
        }

        [TestMethod]
        public void TestScrypt()
        {
            byte[] scryptUtf8Empty = Util.GetBytes(Constants.ScryptUtf8Empty);
            byte[] hash = Hash.Scrypt(new byte[0], new byte[0], 16384, 8, 1, 64);
            CollectionAssert.AreEqual(scryptUtf8Empty, hash);
        }
    }
}
