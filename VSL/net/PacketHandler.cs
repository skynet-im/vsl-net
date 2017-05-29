using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

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
                new P00Handshake(),
                new P01KeyExchange(),
                new P03FinishHandshake(),
                new P04ChangeIV()
            };
        }
        //  constructor>
        // <functions
        internal bool TryGetPacket(byte id, out IPacket packet)
        {
            foreach (IPacket p in RegisteredPackets)
            {
                if (id == p.ID)
                {
                    packet = p;
                    return true;
                }
            }

            packet = null;
            return false;
        }

        /// <summary>
        /// Handles an internal VSL packet
        /// </summary>
        /// <param name="id">Packet ID</param>
        /// <param name="content">Packet data</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        internal void HandleInternalPacket(byte id, byte[] content)
        {
            foreach (IPacket p in RegisteredPackets)
            {
                if (id == p.ID)
                {
                    IPacket packet = p.CreatePacket(content);
                    packet.HandlePacket(this);
                    return;
                }
            }
        }

        internal abstract void HandleP00Handshake(P00Handshake p);
        internal abstract void HandleP01KeyExchange(P01KeyExchange p);
        internal abstract void HandleP02Certificate(P02Certificate p);
        internal abstract void HandleP03FinishHandshake(P03FinishHandshake p);
        internal abstract void HandleP04ChangeIV(P04ChangeIV p);
        internal abstract void HandleP05KeepAlive(P05KeepAlive p);
        internal virtual void HandleP06Accepted(P06Accepted p)
        {
            if (p.RelatedPacket > 5 && p.RelatedPacket < 10)
                parent.FileTransfer.OnAccepted(p);
            else
                throw new InvalidOperationException("Could not resume related packet");
        }
        internal abstract void HandleP07OpenFileTransfer(P07OpenFileTransfer p);
        internal abstract void HandleP08FileHeader(P08FileHeader p);
        internal abstract void HandleP09FileDataBlock(P09FileDataBlock p);
        //  functions>
    }
}