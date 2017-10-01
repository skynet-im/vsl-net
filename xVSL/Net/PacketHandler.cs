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
                //new P02Certificate(),
                new P03FinishHandshake(),
                new P04ChangeIV(),
                //new P05KeepAlive(),
                new P06Accepted(),
                new P07OpenFileTransfer(),
                new P08FileHeader(),
                new P09FileDataBlock()
            };
        }
        //  constructor>
        // <functions
        internal bool TryGetPacket(byte id, out IPacket packet)
        {
            foreach (IPacket p in RegisteredPackets)
            {
                if (id == p.PacketID)
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
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <returns></returns>
        internal void HandleInternalPacket(byte id, byte[] content)
        {
            foreach (IPacket p in RegisteredPackets)
            {
                if (id == p.PacketID)
                {
                    IPacket packet = p.New();
                    PacketBuffer buf = new PacketBuffer(content);
                    packet.ReadPacket(buf);
                    buf.Dispose();
                    packet.HandlePacket(this);
                    return;
                }
            }
        }

        internal abstract bool HandleP00Handshake(P00Handshake p);
        internal abstract bool HandleP01KeyExchange(P01KeyExchange p);
        internal abstract bool HandleP02Certificate(P02Certificate p);
        internal abstract bool HandleP03FinishHandshake(P03FinishHandshake p);
        internal abstract bool HandleP04ChangeIV(P04ChangeIV p);
        internal abstract bool HandleP05KeepAlive(P05KeepAlive p);
        internal virtual bool HandleP06Accepted(P06Accepted p)
        {
            if (p.RelatedPacket > 5 && p.RelatedPacket < 10)
            {
                parent.FileTransfer.OnAccepted(p);
                return true;
            }
            else
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket", string.Format("Could not resume related packet with id {0}.", p.RelatedPacket));
                return false;
            }
        }
        internal abstract bool HandleP07OpenFileTransfer(P07OpenFileTransfer p);
        // TODO: What if no file transfer is running?
        internal virtual bool HandleP08FileHeader(P08FileHeader p)
        {
            parent.FileTransfer.OnHeaderReceived(p);
            return true;
        }
        internal virtual bool HandleP09FileDataBlock(P09FileDataBlock p)
        {
            if (parent.FileTransfer.ReceivingFile)
            {
                parent.FileTransfer.OnDataBlockReceived(p);
                return true;
            }
            else
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket", "No running file transfer that fits to the received P09FileDataBlock");
                return false;
            }
        }
        //  functions>
    }
}