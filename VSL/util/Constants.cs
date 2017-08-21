using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    /// <summary>
    /// Provides general information about VSL.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The installed version of VSL.
        /// </summary>
        public const string ProductVersion = "1.1.15.2";
        /// <summary>
        /// The installed version as ushort.
        /// </summary>
        public const ushort VersionNumber = 1;
        /// <summary>
        /// The oldest supported version of VSL.
        /// </summary>
        public const ushort CompatibilityVersion = 1;
        /// <summary>
        /// The default size of the receive buffer of the Socket.
        /// </summary>
        public const int ReceiveBufferSize = 65536;
        /// <summary>
        /// The count of milliseconds the socket waits to receive a complete packet.
        /// </summary>
        public const int ReceiveTimeout = 5000;
        /// <summary>
        /// The maximum admissible packet size. If a received packet is bigger the receiver closes the connection.
        /// </summary>
        public const int MaxPacketSize = 1048576;
        /// <summary>
        /// The default sleep time for background threads while waiting for work.
        /// </summary>
        public const int SleepTime = 10;
    }
}