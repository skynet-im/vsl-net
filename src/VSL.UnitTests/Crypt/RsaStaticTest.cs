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
        public void TestXmlImportExport()
        {
            RSAParameters @private = RsaStatic.GenerateKeyPairParams();
            RSAParameters @public = RsaStatic.ExtractPublicKey(@private);

            string privateXml = @private.ExportXmlKey();
            string publicXml = @public.ExportXmlKey();

            RSAParameters privateImport = new RSAParameters().ImportXmlKey(privateXml);
            RSAParameters publicImport = new RSAParameters().ImportXmlKey(publicXml);

            CollectionAssert.AreEqual(@private.Modulus, privateImport.Modulus);
            CollectionAssert.AreEqual(@private.Exponent, privateImport.Exponent);
            CollectionAssert.AreEqual(@private.P, privateImport.P);
            CollectionAssert.AreEqual(@private.Q, privateImport.Q);
            CollectionAssert.AreEqual(@private.DP, privateImport.DP);
            CollectionAssert.AreEqual(@private.DQ, privateImport.DQ);
            CollectionAssert.AreEqual(@private.InverseQ, privateImport.InverseQ);
            CollectionAssert.AreEqual(@private.D, privateImport.D);

            CollectionAssert.AreEqual(@public.Modulus, publicImport.Modulus);
            CollectionAssert.AreEqual(@public.Exponent, publicImport.Exponent);
            Assert.IsNull(publicImport.P);
            Assert.IsNull(publicImport.Q);
            Assert.IsNull(publicImport.DP);
            Assert.IsNull(publicImport.DQ);
            Assert.IsNull(publicImport.InverseQ);
            Assert.IsNull(publicImport.D);
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
        public void TestCryptArbitraryLength()
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
