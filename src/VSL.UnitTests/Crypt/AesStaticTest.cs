using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSL.BinaryTools;
using VSL.Crypt;

namespace VSL.UnitTests.Crypt
{
    [TestClass]
    public class AesStaticTest
    {
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
    }
}
