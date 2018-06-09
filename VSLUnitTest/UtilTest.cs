using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSL.BinaryTools;

namespace VSLUnitTest
{
    [TestClass]
    public class UtilTest
    {
        [TestMethod]
        public void TestEquals()
        {
            byte[] test1 = Util.GetBytes(Constants.ScryptUtf8Empty);
            byte[] test2 = Util.GetBytes(Constants.ScryptUtf8Empty);
            byte[] test3 = Util.GetBytes(Constants.Sha256Utf8Empty);

            Assert.IsTrue(test1.SafeEquals(test2));
            Assert.IsTrue(test1.FastEquals(test2));
            Assert.IsFalse(test1.SafeEquals(test3));
            Assert.IsFalse(test1.FastEquals(test3));

            test2[43] ^= 0xff;

            Assert.IsFalse(test1.SafeEquals(test2));
            Assert.IsFalse(test1.FastEquals(test2));
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
            byte[] @checked = Util.GetBytes(Constants.Sha1Utf8Empty);
            byte[] @unchecked = Util.GetBytesUnchecked(Constants.Sha1Utf8Empty);
            string safeStr = Util.ToHexString(@checked);
            string uncheckedStr = Util.ToHexString(@unchecked);
            CollectionAssert.AreEqual(@checked, @unchecked);
            Assert.AreEqual(Constants.Sha1Utf8Empty, safeStr, true);
            Assert.AreEqual(Constants.Sha1Utf8Empty, uncheckedStr, true);
        }
    }
}
