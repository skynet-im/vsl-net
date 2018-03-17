using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Security;

namespace VSL.Crypt
{
    /// <summary>
    /// A static implementation for several hash algorithms.
    /// </summary>
    public static class Hash
    {
        // © 2017-2018 Daniel Lerch
        #region SHA1
        /// <summary>
        /// Computes the SHA1 hash of a string with the specified encoding.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] SHA1(string s, Encoding encoding)
        {
            return SHA1(encoding.GetBytes(s));
        }
        /// <summary>
        /// Computes the SHA1 hash of a byte array.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] SHA1(byte[] buffer)
        {
#if WINDOWS_UWP
            using (var csp = System.Security.Cryptography.SHA1.Create())
#else
            using (var csp = new SHA1CryptoServiceProvider())
#endif
                return csp.ComputeHash(buffer);
        }
        /// <summary>
        /// Computes the SHA1 hash of a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [Obsolete("VSL.Crypt.Hash.SHA1(String) is deprecated. Please use VSL.Crypt.Hash.SHA1File(String) instead.", false)] // deprecated since VSL 1.3
        public static byte[] SHA1(string fileName) => SHA1File(fileName);
        /// <summary>
        /// Computes the SHA1 hash of a file.
        /// </summary>
        /// <param name="path">The path to open a <see cref="FileStream"/> and compute the hash.</param>
        /// <returns></returns>
        public static byte[] SHA1File(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return SHA1(fs);
        }
        /// <summary>
        /// Computes the SHA1 hash of a stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] SHA1(Stream stream)
        {
#if WINDOWS_UWP
            using (var csp = System.Security.Cryptography.SHA1.Create())
#else
            using (var csp = new SHA1CryptoServiceProvider())
#endif
                return csp.ComputeHash(stream);
        }
        #endregion
        #region SHA256
        /// <summary>
        /// Computes the SHA256 hash of a string with the specified encoding.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] SHA256(string s, Encoding encoding)
        {
            return SHA256(encoding.GetBytes(s));
        }
        /// <summary>
        /// Computes the SHA256 hash of a byte array.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public static byte[] SHA256(byte[] buffer)
        {
#if WINDOWS_UWP
            using (var csp = System.Security.Cryptography.SHA256.Create())
#else
            using (var csp = new SHA256CryptoServiceProvider())
#endif
                return csp.ComputeHash(buffer);
        }
        /// <summary>
        /// Computes the SHA256 hash of a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [Obsolete("VSL.Crypt.Hash.SHA256(String) is deprecated. Please use VSL.Crypt.Hash.SHA256File(String) instead.", false)] // deprecated since VSL 1.3
        public static byte[] SHA256(string fileName) => SHA256File(fileName);
        /// <summary>
        /// Computes the SHA256 hash of a file.
        /// </summary>
        /// <param name="path">The path to open a <see cref="FileStream"/> and compute the hash.</param>
        /// <returns></returns>
        public static byte[] SHA256File(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return SHA256(fs);
        }
        /// <summary>
        /// Computes the SHA256 hash of a stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public static byte[] SHA256(Stream stream)
        {
#if WINDOWS_UWP
            using (var csp = System.Security.Cryptography.SHA256.Create())
#else
            using (var csp = new SHA256CryptoServiceProvider())
#endif
                return csp.ComputeHash(stream);
        }
        #endregion
        #region Scrypt
        /// <summary>
        /// This password hashing system tries to thwart off-line password
        /// cracking using a computationally-intensive hashing algorithm.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns>Binary 256bit hash.</returns>
        public static byte[] Scrypt(byte[] password, byte[] salt)
        {
            return Scrypt(password, salt, 16384, 8, 0);
        }
        /// <summary>
        /// This password hashing system tries to thwart off-line password
        /// cracking using a computationally-intensive hashing algorithm,
        /// the work factor of the algorithm is parameterised, so it can be increased
        /// as computers get faster.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <param name="n"></param>
        /// <param name="r"></param>
        /// <param name="p"></param>
        /// <returns>Binary 256bit hash.</returns>
        public static byte[] Scrypt(byte[] password, byte[] salt, int n, int r, int p)
        {
            return ScryptCsp.ComputeHash(password, salt, n, r, p);
        }
#endregion
    }
}