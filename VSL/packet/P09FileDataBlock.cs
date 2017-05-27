using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal class P09FileDataBlock : IPacket
    {
        internal ulong StartPosition;
        internal byte[] DataBlock;

        internal P09FileDataBlock()
        {

        }

        internal P09FileDataBlock(ulong startPosition, byte[] dataBlock)
        {
            StartPosition = startPosition;
            DataBlock = dataBlock;
        }

        public byte ID { get; } = 9;

        public PacketLength PacketLength { get; } = new VariableLength();

        public IPacket CreatePacket(byte[] buf)
        {
            P09FileDataBlock packet = new P09FileDataBlock();
            packet.ReadPacket(buf);
            return packet;
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandleP09FileDataBlock(this);
        }

        public void ReadPacket(byte[] buf)
        {
            PacketBuffer reader = new PacketBuffer(buf);
            StartPosition = reader.ReadULong();
            DataBlock = reader.ReadByteArray(buf.Length - 8);
        }

        public byte[] WritePacket()
        {
            PacketBuffer writer = new PacketBuffer();
            writer.WriteULong(StartPosition);
            writer.WriteByteArray(DataBlock, false);
            return writer.ToArray();
        }
    }
}