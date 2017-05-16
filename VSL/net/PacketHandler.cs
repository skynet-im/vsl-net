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
                new Packet00Handshake(),
                new Packet01KeyExchange(),
                new Packet03FinishHandshake(),
                new Packet04ChangeIV()
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
        internal abstract void HandlePacket01KeyExchange(Packet01KeyExchange p);
        internal abstract void HandlePacket02Certificate(Packet02Certificate p);
        internal abstract void HandlePacket03FinishHandshake(Packet03FinishHandshake p);
        internal abstract void HandlePacket04ChangeIV(Packet04ChangeIV p);
        internal abstract void HandlePacket05KeepAlive(Packet05KeepAlive p);
        //  functions>
    }
}