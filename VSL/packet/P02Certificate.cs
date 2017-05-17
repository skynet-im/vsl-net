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

        public byte ID { get; } = 2;

        public PacketLength Length { get; } = new VariableLength();

        public IPacket CreatePacket(byte[] buf)
        {
            throw new NotImplementedException();
        }

        public void HandlePacket(PacketHandler handler)
        {
            throw new NotImplementedException();
        }

        public void ReadPacket(byte[] buf)
        {
            throw new NotImplementedException();
        }

        public byte[] WritePacket()
        {
            throw new NotImplementedException();
        }
    }
}