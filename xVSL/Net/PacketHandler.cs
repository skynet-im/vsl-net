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
                    if (rule.Algorithms.Contains(alg))
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
            // TODO: Echo an keepalive packet if the last call was more than one second ago.
            return true;
        }
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