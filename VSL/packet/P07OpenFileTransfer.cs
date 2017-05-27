using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal class P07OpenFileTransfer : IPacket
    {
        internal Identifier IdentificationMode;
        internal uint Identifier;

        internal P07OpenFileTransfer()
        {

        }

        public byte ID { get; } = 7;

        public PacketLength PacketLength { get; } = new VariableLength();

        public IPacket CreatePacket(byte[] buf)
        {
            P07OpenFileTransfer packet = new P07OpenFileTransfer();
            packet.ReadPacket(buf);
            return packet;
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandleP07OpenFileTransfer(this);
        }

        public void ReadPacket(byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] WritePacket()
        {
            throw new NotImplementedException();
        }
    }
}