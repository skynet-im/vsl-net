using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Network;

namespace VSL.Packet
{
    internal class P02Certificate : IPacket
    {
        internal P02Certificate()
        {
        }

        public byte PacketId { get; } = 2;

        public uint? ConstantLength => null;

        public IPacket New() => new P02Certificate();

        public Task<bool> HandlePacketAsync(PacketHandler handler)
        {
            return handler.HandleP02Certificate(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
        }

        public void WritePacket(PacketBuffer buf)
        {
        }
    }
}