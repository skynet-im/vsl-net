using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSL.BinaryTools;

namespace VSL.UnitTests.BinaryTools
{
    [TestClass]
    public class UtilTest
    {
        [DataTestMethod]
        [DataRow(true, Constants.ScryptUtf8Empty, Constants.ScryptUtf8Empty)]
        [DataRow(true, Constants.Sha256Utf8Empty, Constants.Sha256Utf8Empty)]
        [DataRow(false, Constants.ScryptUtf8Empty, Constants.Sha256Utf8Empty)]
        [DataRow(false, Constants.ScryptUtf8Empty, Constants.ScryptUtf8Hello)]
        public void TestSafeEquals(bool expected, string left, string right)
        {
            byte[] leftB = Util.GetBytes(left);
            byte[] rightB = Util.GetBytes(right);

            bool actual = leftB.SafeEquals(rightB);

            if (expected)
                Assert.IsTrue(actual);
            else
                Assert.IsFalse(actual);
        }

        [DataTestMethod]
        [DataRow(true, Constants.ScryptUtf8Empty, Constants.ScryptUtf8Empty)]
        [DataRow(true, Constants.Sha256Utf8Empty, Constants.Sha256Utf8Empty)]
        [DataRow(false, Constants.ScryptUtf8Empty, Constants.Sha256Utf8Empty)]
        [DataRow(false, Constants.ScryptUtf8Empty, Constants.ScryptUtf8Hello)]
        public void TestFastEquals(bool expected, string left, string right)
        {
            byte[] leftB = Util.GetBytes(left);
            byte[] rightB = Util.GetBytes(right);

            bool actual = leftB.FastEquals(rightB);

            if (expected)
                Assert.IsTrue(actual);
            else
                Assert.IsFalse(actual);
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
            byte[] result = Util.GetBytes(test);
            CollectionAssert.AreEqual(match, result, "Util.GetBytes(String) returned an unexpected byte array");
        }

        [TestMethod]
        public void TestGetBytesUnsafe()
        {
            string test = "01" + "11" + "2a" + "3e" + "42" + "57" + "fe" + "ff";
            byte[] match = { 0x01, 0x11, 0x2a, 0x3e, 0x42, 0x57, 0xfe, 0xff };
            byte[] result = Util.GetBytesUnchecked(test);
            CollectionAssert.AreEqual(match, result, "Util.GetBytesUnchecked(String) returned an unexpected byte array");
        }

        [TestMethod]
        public void TestHexStringCombined()
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
