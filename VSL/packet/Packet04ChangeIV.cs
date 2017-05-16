using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class Packet04ChangeIV : IPacket
    {
        internal byte[] ClientIV;
        internal byte[] ServerIV;

        internal Packet04ChangeIV()
        {

        }

        internal Packet04ChangeIV(byte[] clientIV, byte[] serverIV)
        {
            ClientIV = clientIV;
            ServerIV = serverIV;
        }

        public byte ID
        {
            get
            {
                return 4;
            }
        }

        public IPacket CreatePacket(byte[] buf)
        {
            Packet04ChangeIV packet = new Packet04ChangeIV();
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