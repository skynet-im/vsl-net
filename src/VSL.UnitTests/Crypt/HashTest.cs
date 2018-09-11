using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using VSL.BinaryTools;
using VSL.Crypt;

namespace VSL.UnitTests.Crypt
{
    [TestClass]
    public class HashTest
    {
        [TestMethod]
        public void TestSha1()
        {
            byte[] sha1Utf8Empty = Util.GetBytes(Constants.Sha1Utf8Empty);
            byte[] sha1 = Hash.SHA1("", Encoding.UTF8);
            CollectionAssert.AreEqual(sha1Utf8Empty, sha1, "Empty SHA-1 hash invalid.");
        }

        [TestMethod]
        public void TestSha256()
        {
            byte[] sha256Utf8Empty = Util.GetBytes(Constants.Sha256Utf8Empty);
            byte[] sha256 = Hash.SHA256("", Encoding.UTF8);
            CollectionAssert.AreEqual(sha256Utf8Empty, sha256, "Empty SHA-256 hash invalid.");
        }

        [DataTestMethod]
        [DataRow("", Constants.ScryptUtf8Empty)]
        [DataRow("Hello", Constants.ScryptUtf8Hello)]
        public void TestScrypt(string input, string result)
        {
            byte[] hash = Hash.Scrypt(Encoding.UTF8.GetBytes(input), new byte[0], 16384, 8, 1, 64);
            CollectionAssert.AreEqual(Util.GetBytes(result), hash);
        }
    }
}
