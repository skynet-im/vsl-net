using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

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

        public byte PacketID { get; } = 0;

        public PacketLength PacketLength { get; } = new ConstantLength(1);

        public IPacket New()
        {
            return new P00Handshake();
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandleP00Handshake(this);
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