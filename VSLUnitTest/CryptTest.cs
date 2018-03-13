using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
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
            Assert.IsTrue(plaintext.SequenceEqual(decrypted));

            // Test multiple blocks
            plaintext = new byte[3456];
            random.NextBytes(plaintext);

            ciphertext = RsaStatic.Encrypt(plaintext, @public);

            decrypted = RsaStatic.Decrypt(ciphertext, @private);
            Assert.IsTrue(plaintext.SequenceEqual(decrypted));
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
            Assert.IsTrue(plaintext.SequenceEqual(decrypted));
        }
    }
}
