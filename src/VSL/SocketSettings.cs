using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using VSL.Crypt;

namespace VSL
{
    /// <summary>
    /// Defines settings for <see cref="VSLSocket"/> derived types.
    /// </summary>
    public class SocketSettings
    {
        /// <summary>
        /// Gets or sets the buffer size for receive and send operation of the underlying socket.
        /// </summary>
        public int ReceiveSendBufferSize { get; set; } = 65536;

        /// <summary>
        /// Gets or sets the maximum admissible packet size. If a received packet is bigger the receiver closes the connection.
        /// </summary>
        public int MaxPacketSize { get; set; } = 1048576;

        /// <summary>
        /// Gets or sets the block size used to transfer files. Per <see cref="Packet.P09FileDataBlock"/> you have at most 65 bytes overhead
        /// </summary>
        public int FTBlockSize { get; set; } = 65536;

        /// <summary>
        /// Gets or sets whether VSL should catch exceptions thrown in an event handler.
        /// </summary>
        public bool CatchApplicationExceptions { get; set; } = true;

        /// <summary>
        /// Gets or sets the latest compatible user product version.
        /// </summary>
        public ushort LatestProductVersion { get; set; } = 0;

        /// <summary>
        /// Gets or sets the oldest compatible user product version.
        /// </summary>
        public ushort OldestProductVersion { get; set; } = 0;

        /// <summary>
        /// Gets or sets the RSA Key for asymmetric key exchange.
        /// </summary>
        public RSAParameters RsaKey { get; set; }

        /// <summary>
        /// Gets or sets the RSA Key in XML format for asymmetric key exchange.
        /// </summary>
        public string RsaXmlKey { get => RsaKey.ExportXmlKey(); set => RsaKey = new RSAParameters().ImportXmlKey(value); }
    }
}
