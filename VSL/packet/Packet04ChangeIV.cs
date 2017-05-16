using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class Packet04ChangeIV : IPacket
    {
        internal Packet04ChangeIV()
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
            return new Packet04ChangeIV();
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandlePacket04ChangeIV(this);
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