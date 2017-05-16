using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class Packet05KeepAlive : IPacket
    {
        internal Packet05KeepAlive()
        {

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
            return new Packet05KeepAlive();
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