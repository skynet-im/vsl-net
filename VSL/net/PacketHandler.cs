using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    /// <summary>
    /// Handles internal VSL packets
    /// </summary>
    internal abstract class PacketHandler
    {
        // <fields
        internal List<IPacket> RegisteredPackets;
        internal VSLSocket parent;
        //  fields>

        // <constructor
        /// <summary>
        /// Initializes all non-child-specific components
        /// </summary>
        internal void InitializeComponent()
        {
            RegisteredPackets = new List<IPacket>
            {
                new Packet255Accepted()
            };
        }
        //  constructor>

        // <functions
        internal virtual bool TryHandlePacket(byte id, byte[] content)
        {
            foreach (IPacket p in RegisteredPackets)
            {
                if (id == p.ID)
                {
                    IPacket packet = p.CreatePacket(content);
                    packet.HandlePacket(this);
                    return true;
                }
            }
            return false;
        }

        internal abstract void HandlePacket00Handshake(Packet00Handshake p);
        internal abstract void HandlePacket255Accepted(Packet255Accepted p);
        //  functions>
    }
}