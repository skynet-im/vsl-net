using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Network;

namespace VSL.Packet
{
    internal class P05KeepAlive : IPacket
    {
        internal KeepAliveRole Role { get; private set; }

        internal P05KeepAlive()
        {

        }

        public P05KeepAlive(KeepAliveRole role)
        {
            Role = role;
        }

        public byte PacketId { get; } = 4;

        public uint? ConstantLength => 0;

        public IPacket New()
        {
            return new P05KeepAlive();
        }

        public bool HandlePacket(PacketHandler handler)
        {
            return handler.HandleP05KeepAlive(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            Role = (KeepAliveRole)buf.ReadByte();
        }

        public void WritePacket(PacketBuffer buf)
        {
            buf.WriteByte((byte)Role);
        }
    }
}