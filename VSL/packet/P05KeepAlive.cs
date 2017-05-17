using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

namespace VSL.Packet
{
    internal class P05KeepAlive : IPacket
    {
        internal P05KeepAlive()
        {

        }

        public byte ID { get; } = 4;

        public PacketLength Length { get; } = new ConstantLength(0);

        public IPacket CreatePacket(byte[] buf)
        {
            return new P05KeepAlive();
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandlePacket05KeepAlive(this);
        }

        public void ReadPacket(byte[] buf)
        {
            
        }

        public byte[] WritePacket()
        {
            return new byte[0];
        }
    }
}