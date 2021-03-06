﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VSL.Network;

namespace VSL.Packet
{
    /// <summary>
    /// Represents an internal VSL packet
    /// </summary>
    internal interface IPacket
    {
        /// <summary>
        /// Returns the packet Id.
        /// </summary>
        byte PacketId { get; }
        /// <summary>
        /// Gets the constant length of this packet or if the length is dynamic.
        /// </summary>
        uint? ConstantLength { get; }
        /// <summary>
        /// Creates a new packet of the specified instance.
        /// </summary>
        IPacket New();
        /// <summary>
        /// Forwards the packet to the handler.
        /// </summary>
        /// <param name="handler">The responsible packet handler.</param>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="NotImplementedException"/>
        /// <exception cref="NotSupportedException"/>
        Task<bool> HandlePacketAsync(PacketHandler handler);
        /// <summary>
        /// Reads the data from a PacketBuffer.
        /// </summary>
        /// <param name="buf">PacketBuffer to read the packet content.</param>
        /// <exception cref="ArgumentException"/>
        void ReadPacket(PacketBuffer buf);
        /// <summary>
        /// Writes the data to a PacketBuffer.
        /// </summary>
        /// <param name="buf">PacketBuffer to write the packet content.</param>
        void WritePacket(PacketBuffer buf);
    }
}