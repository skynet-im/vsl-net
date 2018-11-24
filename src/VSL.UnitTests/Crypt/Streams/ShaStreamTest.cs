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
    public class ShaStreamTest
    {
        [DataTestMethod]
        [DataRow("7343347c8ab8f853d7d79fc4204e30cbf3e55e9fecdbb983290d27b64c11446a")]
        [DataRow("2190febb97c40e3f1f57b147874fac1d72911bec2a741d3fea190808aa8412e0")]
        public void TestWriteMode(string input)
        {
            byte[] buffer = Util.GetBytes(input);
            MemoryStream baseStream = new MemoryStream();
            ShaStream stream = new ShaStream(baseStream, CryptoStreamMode.Write);
            stream.Write(buffer, 0, buffer.Length);
            stream.FlushFinalBlock();
            stream.Dispose();
            Assert.IsFalse(baseStream.CanRead);
            Assert.IsFalse(baseStream.CanWrite);
        }
    }
}
