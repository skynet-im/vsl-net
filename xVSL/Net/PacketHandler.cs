using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Net;
using VSL.Packet;

namespace VSL
{
    /// <summary>
    /// Handles internal VSL packets
    /// </summary>
    internal abstract class PacketHandler
    {
        // <fields
        internal VSLSocket parent;
        protected List<PacketRule> RegisteredPackets;
        //  fields>
        // <functions
        internal bool TryGetPacket(byte id, out IPacket packet)
        {
            foreach (PacketRule rule in RegisteredPackets)
            {
                if (id == rule.Packet.PacketID)
                {
                    packet = rule.Packet;
                    return true;
                }
            }

            packet = null;
            return false;
        }

        /// <summary>
        /// Handles an internal VSL packet.
        /// </summary>
        /// <param name="id">Packet ID.</param>
        /// <param name="content">Packet data.</param>
        /// <param name="alg">Algorithm that was detected for this packet.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <returns></returns>
        internal bool HandleInternalPacket(byte id, byte[] content, CryptoAlgorithm alg)
        {
            foreach (PacketRule rule in RegisteredPackets)
            {
                if (id == rule.Packet.PacketID)
                {
                    if (rule.Algorithms.Contains(alg)) // TODO: [VSL 1.2.2] Contains needs O(n) for a search, a dictionary or an array<bool> would be much faster
                    {
                        IPacket packet = rule.Packet.New();
                        PacketBuffer buf = new PacketBuffer(content);
                        packet.ReadPacket(buf);
                        buf.Dispose();
                        return packet.HandlePacket(this);
                    }
                    else
                    {
                        parent.ExceptionHandler.CloseConnection("WrongAlgorithm", $"Received {rule.Packet.ToString()} with {alg.ToString()} which is not allowed.");
                        return false;
                    }
                }
            }
            parent.ExceptionHandler.CloseConnection("UnknownPacket", $"Packet id {id} is not an internal packet and cannot be handled\r\n\tat PacketHandler.HandleInternalPacket()");
            return false;
        }

        internal abstract bool HandleP00Handshake(P00Handshake p);
        internal abstract bool HandleP01KeyExchange(P01KeyExchange p);
        internal abstract bool HandleP02Certificate(P02Certificate p);
        internal abstract bool HandleP03FinishHandshake(P03FinishHandshake p);
        internal abstract bool HandleP04ChangeIV(P04ChangeIV p);
        internal virtual bool HandleP05KeepAlive(P05KeepAlive p)
        {
            if (parent.ConnectionVersion.Value < 2)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "VSL 1.1 and lower versions do not support keep-alive packets.");
                return false;
            }
            if (p.Role == KeepAliveRole.Request)
                return parent.manager.SendPacket(new P05KeepAlive(KeepAliveRole.Response));
            else
                return true;
            // TODO: [VSL 1.2.2] API and timeout for keep-alives
        }
        internal bool HandleP06Accepted(P06Accepted p)
        {
            if (p.RelatedPacket > 5 && p.RelatedPacket < 10)
            {
                return parent.FileTransfer.OnPacketReceived(p);
            }
            else
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket", string.Format("Could not resume related packet with id {0}.", p.RelatedPacket));
                return false;
            }
        }
        internal virtual bool HandleP07OpenFileTransfer(P07OpenFileTransfer p)
        {
            if (parent.ConnectionVersion.Value < 2)
            {
                byte original = p.StreamMode.InverseToByte();
                if (original == 2) // StreamMode.PushHeader (zero based index 2) is not implemented in VSL 1.1
                    original++;
                p.StreamMode = FileTransfer.StreamMode.InverseFromByte(original);
            }
            return parent.FileTransfer.OnPacketReceived(p);
        }
        internal bool HandleP08FileHeader(P08FileHeader p)
        {
            return parent.FileTransfer.OnPacketReceived(p);
        }
        internal bool HandleP09FileDataBlock(P09FileDataBlock p)
        {
            return parent.FileTransfer.OnPacketReceived(p);
        }
        //  functions>
    }
}