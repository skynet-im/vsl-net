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
            PacketBuffer buf = new PacketBuffer(data);
            Identifier = FileTransfer.Identifier.FromBinary(buf);
            StreamMode = (StreamMode)buf.ReadByte();
        }

        public byte[] WritePacket()
        {
            PacketBuffer buf = new PacketBuffer();
            Identifier.ToBinary(buf);
            buf.WriteByte((byte)StreamMode);
            return buf.ToArray();
        }
    }
}