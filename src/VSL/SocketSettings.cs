using System;
using System.Collections.Generic;
using System.Text;

namespace VSL
{
    /// <summary>
    /// Defines settings for <see cref="VSLSocket"/> derived types.
    /// </summary>
    public class SocketSettings
    {
        /// <summary>
        /// Gets the default size of the receive buffer of the Socket.
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 65536;

        /// <summary>
        /// Gets the maximum admissible packet size. If a received packet is bigger the receiver closes the connection.
        /// </summary>
        public int MaxPacketSize { get; set; } = 1048576;

        /// <summary>
        /// Gets or sets whether VSL should catch exceptions thrown in an event handler.
        /// </summary>
        public bool CatchApplicationExceptions { get; set; } = true;
    }
}
