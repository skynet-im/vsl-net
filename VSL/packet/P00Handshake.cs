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

        public byte ID { get; } = 0;

        public PacketLength Length { get; } = new ConstantLength(1);

        public IPacket CreatePacket(byte[] buf)
        {
            P00Handshake packet = new P00Handshake();
            packet.ReadPacket(buf);
            return packet;
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandlePacket00Handshake(this);
        }

        public void ReadPacket(byte[] buf)
        {
            PacketBuffer reader = new PacketBuffer(buf);
            RequestType = (RequestType)reader.ReadByte();
        }

        public byte[] WritePacket()
        {
            PacketBuffer writer = new PacketBuffer();
            writer.WriteByte((byte)RequestType);
            return writer.ToArray();
        }
    }
}