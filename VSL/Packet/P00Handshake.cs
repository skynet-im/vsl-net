using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal class P00Handshake : IPacket
    {
        internal RequestType RequestType;

        internal P00Handshake()
        {

        }

        internal P00Handshake(RequestType requestType)
        {
            RequestType = requestType;
        }

        public byte PacketId { get; } = 0;

        public uint? ConstantLength => 1;

        public IPacket New()
        {
            return new P00Handshake();
        }

        public bool HandlePacket(PacketHandler handler)
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