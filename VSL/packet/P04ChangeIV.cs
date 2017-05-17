using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

namespace VSL.Packet
{
    internal class P04ChangeIV : IPacket
    {
        internal byte[] ClientIV;
        internal byte[] ServerIV;

        internal P04ChangeIV()
        {

        }

        internal P04ChangeIV(byte[] clientIV, byte[] serverIV)
        {
            ClientIV = clientIV;
            ServerIV = serverIV;
        }

        public byte ID { get; } = 4;

        public PacketLength Length { get; } = new ConstantLength(32);

        public IPacket CreatePacket(byte[] buf)
        {
            P04ChangeIV packet = new P04ChangeIV();
            packet.ReadPacket(buf);
            return packet;
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandlePacket04ChangeIV(this);
        }

        public void ReadPacket(byte[] buf)
        {
            PacketBuffer reader = new PacketBuffer(buf);
            ClientIV = reader.ReadByteArray(16);
            ServerIV = reader.ReadByteArray(16);
        }

        public byte[] WritePacket()
        {
            PacketBuffer writer = new PacketBuffer();
            writer.WriteByteArray(ClientIV, false);
            writer.WriteByteArray(ServerIV, false);
            return writer.ToArray();
        }
    }
}