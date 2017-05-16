using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class Packet02Certificate : IPacket
    {
        internal Packet02Certificate()
        {

        }

        public byte ID
        {
            get
            {
                return 2;
            }
        }

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