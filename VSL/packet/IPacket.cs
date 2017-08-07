﻿using System;
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
        /// Returns the packet ID.
        /// </summary>
        byte PacketID { get; }
        /// <summary>
        /// Returns the length characteristics of the packet.
        /// </summary>
        PacketLength PacketLength { get; }
        /// <summary>
        /// Creates a new packet of the specified instance.
        /// </summary>
        /// <returns></returns>
        IPacket New();
        /// <summary>
        /// Forwards the packet to the handler.
        /// </summary>
        /// <param name="handler">The responsible packet handler.</param>
        void HandlePacket(PacketHandler handler);
        /// <summary>
        /// Reads the data from a PacketBuffer.
        /// </summary>
        /// <param name="buf">PacketBuffer to read the packet content.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        void ReadPacket(PacketBuffer buf);
        /// <summary>
        /// Writes the data to a PacketBuffer.
        /// </summary>
        /// <param name="buf">PacketBuffer to write the packet content.</param>
        void WritePacket(PacketBuffer buf);
    }
}