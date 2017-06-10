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

        public byte PacketID { get; } = 4;

        public PacketLength PacketLength { get; } = new ConstantLength(0);

        public IPacket New()
        {
            return new P05KeepAlive();
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandleP05KeepAlive(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            
        }

        public void WritePacket(PacketBuffer buf)
        {
            
        }
    }
}