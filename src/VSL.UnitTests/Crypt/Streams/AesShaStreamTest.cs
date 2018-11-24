using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VSL.BinaryTools;
using VSL.Crypt.Streams;

namespace VSL.UnitTests.Crypt.Streams
{
    [TestClass]
    public class AesShaStreamTest
    {
        [DataTestMethod] // 16 bytes iv + 16 bytes aes(iv.take(8), iv)
        [DataRow("e3c2544f6aad8276620a472b23895832c21bee8f23f37f011ecb3790c77378fa6a145e1429517f8d6efa9e433dc9e038")]
        [DataRow("0cdeb0b9bd5dcebffe29dbb40ae83dd6b3f73ad02adcef5752ec94b8db1774356503f53abf111fb4ee2cee5e8459d6fe")]
        public void TestDecryptWriteMode(string input)
        {
            byte[] ciphertext = Util.GetBytes(input);
            byte[] key = Util.GetBytes("fe627bd19db579ddd5e4ce42046f45b2fb708d98a6c6230c315df2760d4966ea");

            MemoryStream baseStream = new MemoryStream();
            AesShaStream stream = new AesShaStream(baseStream, key, CryptoStreamMode.Write, CryptographicOperation.Decrypt);
            stream.Write(ciphertext, 0, ciphertext.Length);
            stream.FlushFinalBlock();
            stream.Dispose();
            Assert.IsFalse(baseStream.CanRead);
            Assert.IsFalse(baseStream.CanWrite);
        }

        [DataTestMethod] // 16 bytes iv + 16 bytes aes(iv.take(8), iv)
        [DataRow("e3c2544f6aad8276620a472b23895832c21bee8f23f37f011ecb3790c77378fa6a145e1429517f8d6efa9e433dc9e038")]
        [DataRow("0cdeb0b9bd5dcebffe29dbb40ae83dd6b3f73ad02adcef5752ec94b8db1774356503f53abf111fb4ee2cee5e8459d6fe")]
        public void TestDecryptWriteModeFragmented(string input)
        {
            byte[] ciphertext = Util.GetBytes(input);
            byte[] key = Util.GetBytes("fe627bd19db579ddd5e4ce42046f45b2fb708d98a6c6230c315df2760d4966ea");

            MemoryStream baseStream = new MemoryStream();
            AesShaStream stream = new AesShaStream(baseStream, key, CryptoStreamMode.Write, CryptographicOperation.Decrypt);
            stream.Write(ciphertext, 0, ciphertext.Length / 2);
            Assert.ThrowsException<CryptographicException>(() => stream.FlushFinalBlock());
            stream.Write(ciphertext, ciphertext.Length / 2, ciphertext.Length / 2);
            stream.FlushFinalBlock();
            stream.Dispose();
            Assert.IsFalse(baseStream.CanRead);
            Assert.IsFalse(baseStream.CanWrite);
        }

        [DataTestMethod] // 16 bytes iv + 16 bytes aes(iv.take(8), iv)
        [DataRow("e3c2544f6aad8276620a472b23895832c21bee8f23f37f011ecb3790c77378fa6a145e1429517f8d6efa9e433dc90000")]
        [DataRow("0cdeb0b9bd5dcebffe29dbb40ae83dd6b3f73ad02adcef5752ec94b8db1774356503f53abf111fb4ee2cee5e84590000")]
        public void TestDecryptWriteModeFail(string input)
        {
            byte[] ciphertext = Util.GetBytes(input);
            byte[] key = Util.GetBytes("fe627bd19db579ddd5e4ce42046f45b2fb708d98a6c6230c315df2760d4966ea");

            MemoryStream baseStream = new MemoryStream();
            AesShaStream stream = new AesShaStream(baseStream, key, CryptoStreamMode.Write, CryptographicOperation.Decrypt);
            stream.Write(ciphertext, 0, ciphertext.Length);
            Assert.ThrowsException<CryptographicException>(() => stream.FlushFinalBlock());
            Assert.ThrowsException<CryptographicException>(() => stream.Dispose());
            Assert.IsFalse(baseStream.CanRead);
            Assert.IsFalse(baseStream.CanWrite);
        }
    }
}
