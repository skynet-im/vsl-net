using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.FileTransfer;
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
        //  fields>

        // <constructor (helper)
        protected static PacketRule[] InitRules(params PacketRule[] rules)
        {
            PacketRule[] final = new PacketRule[10];
            foreach (PacketRule rule in rules)
            {
                final[rule.Packet.PacketId] = rule;
            }
            return final;
        }
        //  constructor>

        // <properties
        protected abstract PacketRule[] RegisteredPackets { get; }
        //  properties>

        // <functions
        internal bool IsInternalPacket(byte id) => id < RegisteredPackets.Length;

        /// <summary>
        /// Validates an internal packet and returns the matching <see cref="PacketRule"/>.
        /// </summary>
        internal bool ValidatePacket(byte id, CryptoAlgorithm alg, out PacketRule rule)
        {
            rule = RegisteredPackets[id];
            if (!rule.Available)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    $"Packet id {id} is no valid internal packet for this instance.\r\n" +
                    "\tat PacketHandler.ValidatePacket(Byte, CryptoAlgorithm, PacketRule)");
                return false;
            }
            if (!rule.VerifyAlgorithm(alg))
            {
                parent.ExceptionHandler.CloseConnection("WrongAlgorithm",
                    $"{rule.Packet} with {alg} is not allowed.\r\n" +
                    "\tat PacketHandler.ValidatePacket(Byte, CryptoAlgorithm, PacketRule)");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Handles an internal VSL packet. Ensure using the correct <see cref="CryptoAlgorithm"/>.
        /// </summary>
        internal bool HandleInternalPacket(PacketRule rule, byte[] content)
        {
            IPacket packet = rule.Packet.New();
            try
            {
                using (PacketBuffer buf = PacketBuffer.CreateStatic(content))
                    packet.ReadPacket(buf);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
            return packet.HandlePacket(this);
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
                    "VSL 1.1 and lower versions do not support keep-alive packets.\r\n" +
                    "\tat PacketHandler.HandleP05KeepAlive(P05KeepAlive)");
                return false;
            }
            if (p.Role == KeepAliveRole.Request)
                return parent.manager.SendPacket(new P05KeepAlive(KeepAliveRole.Response));
            else
                return true;
            // TODO: [VSL 1.2.3] API and timeout for keep-alives
        }
        internal bool HandleP06Accepted(P06Accepted p)
        {
            if (p.RelatedPacket > 5 && p.RelatedPacket < 10)
            {
                return parent.FileTransfer.OnPacketReceived(p);
            }
            else
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    $"Could not resume related packet with id {p.RelatedPacket}.\r\n" +
                    "\tat PacketHandler.HandleP06Accepted(P06Accepted)");
                return false;
            }
        }
        internal virtual bool HandleP07OpenFileTransfer(P07OpenFileTransfer p)
        {
            p.ReverseStreamMode(parent.ConnectionVersion.Value);
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