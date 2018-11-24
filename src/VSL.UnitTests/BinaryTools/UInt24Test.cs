using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSL.BinaryTools;

namespace VSL.UnitTests.BinaryTools
{
    [TestClass]
    public class UInt24Test
    {
        [DataTestMethod]
        [DataRow("000000", 0, 0U)]
        [DataRow("003275670000", 1, 0x677532U)]
        public void TestFromBytes(string hexbuffer, int index, uint expected)
        {
            byte[] buffer = Util.GetBytes(hexbuffer);
            uint actual = UInt24.FromBytes(buffer, index);
            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow("000000", 0, 0U)]
        [DataRow("003275670000", 1, 0x677532U)]
        public void TestToBytes(string hexbuffer, int index, uint input)
        {
            byte[] expected = Util.GetBytes(hexbuffer);
            byte[] buffer = new byte[expected.Length];
            UInt24.ToBytes(input, buffer, index);
            CollectionAssert.AreEqual(expected, buffer);
        }

        [TestMethod]
        public void TestArgumentValidation()
        {
            byte[] buffer = new byte[5];
            Random random = new Random();
            random.NextBytes(buffer);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => UInt24.FromBytes(buffer, 3));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => UInt24.ToBytes(2345325U, buffer, 3));
        }
    }
}
