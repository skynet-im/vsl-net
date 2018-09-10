using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Network
{
    /// <summary>
    /// Provides a set of cryptographic algorithms for network encryption.
    /// </summary>
    internal enum CryptoAlgorithm : byte
    {
        None,
        /// <summary>
        /// RSA-2048 with Optimal Asymmetric Encryption Padding.
        /// </summary>
        RSA_2048_OAEP,
        /// <summary>
        /// [Insecure] AES-256 CBC with split packets.
        /// </summary>
        AES_256_CBC_SP,
        /// <summary>
        /// AES-256 CBC with HMAC-SHA256, multipacket mode and 3byte length marker.
        /// </summary>
        AES_256_CBC_HMAC_SHA256_MP3
    }
}