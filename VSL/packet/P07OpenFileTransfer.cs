using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.FileTransfer;

namespace VSL.Packet
{
    internal class P07OpenFileTransfer : IPacket
    {
        internal Identifier Identifier;
        internal StreamMode StreamMode;

        internal P07OpenFileTransfer()
        {

        }

        internal P07OpenFileTransfer(Identifier identifier, StreamMode streamMode)
        {
            Identifier = identifier;
            StreamMode = streamMode;
        }

        public byte PacketID { get; } = 7;

        public PacketLength PacketLength { get; } = new VariableLength();

        public IPacket New()
        {
            return new P07OpenFileTransfer();
        }

        public bool HandlePacket(PacketHandler handler)
        {
            return handler.HandleP07OpenFileTransfer(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            Identifier = Identifier.FromBinary(buf);
            StreamMode = (StreamMode)buf.ReadByte();
        }

        public void WritePacket(PacketBuffer buf)
        {
            Identifier.ToBinary(buf);
            buf.WriteByte((byte)StreamMode);
        }
    }
}