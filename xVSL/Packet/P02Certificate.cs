using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

namespace VSL.Packet
{
    internal class P02Certificate : IPacket
    {
        internal P02Certificate()
        {

        }

        public byte PacketID { get; } = 2;

        public PacketLength PacketLength { get; } = new VariableLength();

        public IPacket New()
        {
            throw new NotImplementedException();
        }

        public void HandlePacket(PacketHandler handler)
        {
            throw new NotImplementedException();
        }

        public void ReadPacket(PacketBuffer buf)
        {
            throw new NotImplementedException();
        }

        public void WritePacket(PacketBuffer buf)
        {
            throw new NotImplementedException();
        }
    }
}