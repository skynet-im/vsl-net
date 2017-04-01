using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VSL.Crypt
{
    public static class Hash
    {
        // v9 © 2017 Daniel Lerch
        /// <summary>
        /// Berechnet den SHA256-Hash eines Strings
        /// </summary>
        /// <param name="s">String (UTF8-Encoding)</param>
        /// <returns></returns>
        public static byte[] SHA256(string s)
        {
            UTF8Encoding enc = new UTF8Encoding();
            return SHA256(enc.GetBytes(s));
        }
        /// <summary>
        /// Berechnet den SHA256-Hash eines Bytearrays
        /// </summary>
        /// <param name="b">Bytearray</param>
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