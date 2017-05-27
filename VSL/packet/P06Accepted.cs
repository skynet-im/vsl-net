using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal class P06Accepted : IPacket
    {
        internal P06Accepted()
        {

        }

        public byte ID { get; } = 6;

        public PacketLength PacketLength { get; } = new ConstantLength(3);

        public IPacket CreatePacket(byte[] buf)
        {
            P06Accepted packet = new P06Accepted();
            packet.ReadPacket(buf);
            return packet;
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