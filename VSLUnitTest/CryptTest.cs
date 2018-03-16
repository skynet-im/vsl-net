using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using VSL.Crypt;

namespace VSLUnitTest
{
    [TestClass]
    public class CryptTest
    {
        const string Sha1Utf8Empty = "da39a3ee5e6b4b0d3255bfef95601890afd80709";
        const string Sha256Utf8Empty = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

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
        public void TestToHexString()
        {
            byte[] test = { 0x01, 0x11, 0x2a, 0x3e, 0x42, 0x57, 0xfe, 0xff };
            string match = "01" + "11" + "2a" + "3e" + "42" + "57" + "fe" + "ff";
            string result = Util.ToHexString(test);
            Assert.AreEqual(match, result, true);
        }

        [TestMethod]
        public void TestGetBytes()
        {
            string test = "01" + "11" + "2a" + "3e" + "42" + "57" + "fe" + "ff";
            byte[] match = { 0x01, 0x11, 0x2a, 0x3e, 0x42, 0x57, 0xfe, 0xff };
            byte[] @checked = Util.GetBytes(test);
            byte[] @unchecked = Util.GetBytesUnchecked(test);
            CollectionAssert.AreEqual(match, @checked, "Util.GetBytes(String) returned an unexpected byte array");
            CollectionAssert.AreEqual(match, @unchecked, "Util.GetBytesUnchecked(String) returned an unexpected byte array");
        }

        [TestMethod]
        public void TestHexString()
        {
            byte[] @checked = Util.GetBytes(Sha1Utf8Empty);
            byte[] @unchecked = Util.GetBytesUnchecked(Sha1Utf8Empty);
            string safeStr = Util.ToHexString(@checked);
            string uncheckedStr = Util.ToHexString(@unchecked);
            CollectionAssert.AreEqual(@checked, @unchecked);
            Assert.AreEqual(Sha1Utf8Empty, safeStr, true);
            Assert.AreEqual(Sha1Utf8Empty, uncheckedStr, true);
        }

        [TestMethod]
        public void TestHash()
        {
            byte[] sha1Utf8Empty = Util.GetBytes(Sha1Utf8Empty);
            byte[] sha256Utf8Empty = Util.GetBytes(Sha256Utf8Empty);

            byte[] sha1 = Hash.SHA1("", Encoding.UTF8);
            CollectionAssert.AreEqual(sha1Utf8Empty, sha1, "Empty SHA-1 hash invalid.");

            byte[] sha256 = Hash.SHA256("", Encoding.UTF8);
            CollectionAssert.AreEqual(sha256Utf8Empty, sha256, "Empty SHA-256 hash invalid.");
        }
    }
}
