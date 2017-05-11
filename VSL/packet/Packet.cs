using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    /// <summary>
    /// Represents an internal VSL packet
    /// </summary>
    internal interface IPacket
    {
        /// <summary>
        /// Returns the packet ID
        /// </summary>
        byte ID { get; }
        /// <summary>
        /// Reads the data from the byte array
        /// </summary>
        /// <param name="buf">packet content</param>
        void ReadPacket(byte[] buf);
        /// <summary>
        /// Creates a new packet of the specified type
        /// </summary>
        /// <param name="buf">Packet content</param>
        /// <returns></returns>
        IPacket CreatePacket(byte[] buf);
        /// <summary>
        /// Writes the data to a byte array
        /// </summary>
        /// <returns></returns>
        byte[] WritePacket();
        /// <summary>
        /// Forwards the packet to the handler
        /// </summary>
        /// <param name="handler">The responsible packet handler</param>
        void HandlePacket(PacketHandler handler);
    }
}