using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Text;
using VSL.Crypt;

namespace VSL.UnitTests.Crypt
{
    [TestClass]
    public class RsaStaticTest
    {
        [TestMethod]
        public void TestRsaParamsGeneration()
        {
            RSAParameters @private = RsaStatic.GenerateKeyPairParams();
            RSAParameters @public = RsaStatic.ExtractPublicKey(@private);

            Assert.IsNotNull(@private.Modulus);
            Assert.IsNotNull(@private.Exponent);
            Assert.IsNotNull(@private.P);
            Assert.IsNotNull(@private.Q);
            Assert.IsNotNull(@private.DP);
            Assert.IsNotNull(@private.DQ);
            Assert.IsNotNull(@private.InverseQ);
            Assert.IsNotNull(@private.D);

            Assert.IsNotNull(@public.Modulus);
            Assert.IsNotNull(@public.Exponent);
            Assert.IsNull(@public.P);
            Assert.IsNull(@public.Q);
            Assert.IsNull(@public.DP);
            Assert.IsNull(@public.DQ);
            Assert.IsNull(@public.InverseQ);
            Assert.IsNull(@public.D);
        }

        [TestMethod]
        public void TestXmlKeyGeneration()
        {
            string @private = RsaStatic.GenerateKeyPairXml();
            string @public = RsaStatic.ExtractPublicKey(@private);

            Assert.IsNotNull(@private);
            Assert.IsNotNull(@public);
            Assert.AreNotEqual(@private, @public, true);
            Assert.AreNotEqual(@private.Length, @public.Length);
        }

        [TestMethod]
        public void TestCryptBlock()
        {
            RSAParameters @private = RsaStatic.GenerateKeyPairParams();
            RSAParameters @public = RsaStatic.ExtractPublicKey(@private);

            byte[] plaintext = Encoding.UTF8.GetBytes("Some test for cryptographic operations");
            byte[] ciphertext = RsaStatic.EncryptBlock(plaintext, @public);
            byte[] decrypted = RsaStatic.DecryptBlock(ciphertext, @private);

            CollectionAssert.AreEqual(plaintext, decrypted);
        }

        [TestMethod]
        public void TestCryptMore()
        {
            RSAParameters @private = RsaStatic.GenerateKeyPairParams();
            RSAParameters @public = RsaStatic.ExtractPublicKey(@private);

            byte[] plaintext = new byte[734]; // exceed the maximum of 214 bytes
            new Random().NextBytes(plaintext);
            byte[] ciphertext = RsaStatic.Encrypt(plaintext, @public);
            byte[] decrypted = RsaStatic.Decrypt(ciphertext, @private);

            CollectionAssert.AreEqual(plaintext, decrypted);
        }
    }
}
