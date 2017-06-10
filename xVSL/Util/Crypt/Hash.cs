using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VSL.Crypt
{
    /// <summary>
    /// The simple implementation of SHA 1 and SHA256 in VSL
    /// </summary>
    public static class Hash
    {
        // © 2017 Daniel Lerch
        /// <summary>
        /// Computes the SHA1 hash of a string (not collision safe)
        /// </summary>
        /// <param name="s">string (UTF8 encoding)</param>
        /// <returns></returns>
        public static byte[] SHA1(string s)
        {
            UTF8Encoding enc = new UTF8Encoding();
            return SHA1(enc.GetBytes(s));
        }
        /// <summary>
        /// Computes the SHA1 hash of a byte array (not collision safe)
        /// </summary>
        /// <param name="b">byte array to hash</param>
        /// <returns></returns>
        public static byte[] SHA1(byte[] b)
        {
            byte[] hash;
            using (SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider())
            {
                hash = sha.ComputeHash(b);
            }
            return hash;
        }
        /// <summary>
        /// Computes the SHA256 hash of a string
        /// </summary>
        /// <param name="s">string (UTF8 encoding)</param>
        /// <returns></returns>
        public static byte[] SHA256(string s)
        {
            UTF8Encoding enc = new UTF8Encoding();
            return SHA256(enc.GetBytes(s));
        }
        /// <summary>
        /// Computes the SHA256 hash of a byte array
        /// </summary>
        /// <param name="b">byte array to hash</param>
        /// <returns></returns>
        public static byte[] SHA256(byte[] b)
        {
            byte[] hash;
            using (SHA256CryptoServiceProvider sha = new SHA256CryptoServiceProvider())
            {
                hash = sha.ComputeHash(b);
            }
            return hash;
        }
    }
}