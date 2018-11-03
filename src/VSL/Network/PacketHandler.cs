using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

namespace VSL.Network
{
    /// <summary>
    /// Handles internal VSL packets
    /// </summary>
    internal abstract class PacketHandler
    {
        // <fields
        protected readonly VSLSocket parent;
        //  fields>

        // <constructor
        protected PacketHandler(VSLSocket parent)
        {
            this.parent = parent;
        }

        protected static PacketRule[] InitRules(params PacketRule[] rules)
        {
            PacketRule[] final = new PacketRule[Constants.InternalPacketCount];
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
        internal bool IsInternalPacket(byte id) => id < Constants.InternalPacketCount;

        /// <summary>
        /// Validates an internal packet and returns the matching <see cref="PacketRule"/>.
        /// </summary>
        internal bool ValidatePacket(byte id, CryptoAlgorithm alg, out PacketRule rule)
        {
            rule = RegisteredPackets[id];
            if (!rule.Available)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    $"Packet id {id} is no valid internal packet for this instance.",
                    nameof(PacketHandler), nameof(ValidatePacket));
                return false;
            }
            if (!rule.VerifyAlgorithm(alg))
            {
                parent.ExceptionHandler.CloseConnection("WrongAlgorithm",
                    $"{rule.Packet} with {alg} is not allowed.",
                    nameof(PacketHandler), nameof(ValidatePacket));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Handles an internal VSL packet. Ensure using the correct <see cref="CryptoAlgorithm"/>.
        /// </summary>
        internal Task<bool> HandleInternalPacketAsync(PacketRule rule, byte[] content)
        {
            IPacket packet = rule.Packet.New();
            try
            {
                using (PacketBuffer buf = PacketBuffer.CreateStatic(content))
                    packet.ReadPacket(buf);
                return packet.HandlePacketAsync(this);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return Task.FromResult(false);
            }
        }

        internal abstract Task<bool> HandleP00Handshake(P00Handshake p);
        internal abstract Task<bool> HandleP01KeyExchange(P01KeyExchange p);
        internal abstract Task<bool> HandleP02Certificate(P02Certificate p);
        internal abstract Task<bool> HandleP03FinishHandshake(P03FinishHandshake p);
        internal abstract Task<bool> HandleP04ChangeIV(P04ChangeIV p);
        internal virtual async Task<bool> HandleP05KeepAlive(P05KeepAlive p)
        {
            if (parent.ConnectionVersion.Value < 2)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "VSL 1.1 and lower versions do not support keep-alive packets.",
                    nameof(PacketHandler), nameof(HandleP05KeepAlive));
                return false;
            }
            if (p.Role == KeepAliveRole.Request)
                return await parent.Manager.SendPacketAsync(new P05KeepAlive(KeepAliveRole.Response));
            else
                return true;
            // TODO: [VSL 1.2.3] API and timeout for keep-alives
        }
        internal async Task<bool> HandleP06Accepted(P06Accepted p)
        {
            if (p.RelatedPacket > 5 && p.RelatedPacket < 10)
            {
                return await parent.FileTransfer.OnPacketReceivedAsync(p);
            }
            else
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    $"Could not resume related packet with id {p.RelatedPacket}.",
                    nameof(PacketHandler), nameof(HandleP06Accepted));
                return false;
            }
        }
        internal virtual Task<bool> HandleP07OpenFileTransfer(P07OpenFileTransfer p)
        {
            p.PrepareReceive(parent.ConnectionVersion.Value);
            return parent.FileTransfer.OnPacketReceivedAsync(p);
        }
        internal Task<bool> HandleP08FileHeader(P08FileHeader p)
        {
            return parent.FileTransfer.OnPacketReceivedAsync(p);
        }
        internal Task<bool> HandleP09FileDataBlock(P09FileDataBlock p)
        {
            return parent.FileTransfer.OnPacketReceivedAsync(p);
        }
        //  functions>
    }
}