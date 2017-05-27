using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
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
        /// Returns the length characteristics of the packet
        /// </summary>
        PacketLength PacketLength { get; }
        /// <summary>
        /// Creates a new packet and reads the data
        /// </summary>
        /// <param name="buf">Packet content</param>
        /// <returns></returns>
        IPacket CreatePacket(byte[] buf);
        /// <summary>
        /// Forwards the packet to the handler
        /// </summary>
        /// <param name="handler">The responsible packet handler</param>
        void HandlePacket(PacketHandler handler);
        /// <summary>
        /// Reads the data from the byte array
        /// </summary>
        /// <param name="data">packet content</param>
        void ReadPacket(byte[] data);
        /// <summary>
        /// Writes the data to a byte array
        /// </summary>
        /// <returns></returns>
        byte[] WritePacket();
    }
}