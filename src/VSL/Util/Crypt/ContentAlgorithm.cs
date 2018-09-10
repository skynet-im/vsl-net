using System;
using System.Collections.Generic;
using System.Text;

namespace VSL.Crypt
{
    /// <summary>
    /// Defines cryptographic algorithms for end to end content encryption.
    /// </summary>
    public enum ContentAlgorithm : byte
    {
        /// <summary>
        /// No encryption, content will be handled as plaintext.
        /// </summary>
        None,
        /// <summary>
        /// Encryption using AES 256 with Cipher Block Chaining.
        /// </summary>
        Aes256Cbc,
        /// <summary>
        /// Encryption using AES 256 with Cipher Block Chaining and verifying with HMAC SHA256.
        /// </summary>
        Aes256CbcHmacSha256
    }
}
