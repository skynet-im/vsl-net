using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal class P02Certificate : IPacket
    {
        internal P02Certificate()
        {

        }

        public byte PacketId { get; } = 2;

        public uint? ConstantLength => null;

        public IPacket New()
        {
            return new P02Certificate();
        }

        public bool HandlePacket(PacketHandler handler)
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