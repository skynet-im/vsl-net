using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Network;

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

        public byte PacketId { get; } = 4;

        public uint? ConstantLength => 32;

        public IPacket New()
        {
            return new P04ChangeIV();
        }

        public bool HandlePacket(PacketHandler handler)
        {
            return handler.HandleP04ChangeIV(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            ClientIV = buf.ReadByteArray(16);
            ServerIV = buf.ReadByteArray(16);
        }

        public void WritePacket(PacketBuffer buf)
        {
            buf.WriteByteArray(ClientIV, false);
            buf.WriteByteArray(ServerIV, false);
        }
    }
}