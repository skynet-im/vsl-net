using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Network;

namespace VSL.Packet
{
    internal class P00Handshake : IPacket
    {
        internal RequestType RequestType { get; private set; }

        internal P00Handshake()
        {
        }

        internal P00Handshake(RequestType requestType)
        {
            RequestType = requestType;
        }

        public byte PacketId { get; } = 0;

        public uint? ConstantLength => 1;

        public IPacket New() => new P00Handshake();

        public Task<bool> HandlePacketAsync(PacketHandler handler)
        {
            return handler.HandleP00Handshake(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            RequestType = (RequestType)buf.ReadByte();
        }

        public void WritePacket(PacketBuffer buf)
        {
            buf.WriteByte((byte)RequestType);
        }
    }
}