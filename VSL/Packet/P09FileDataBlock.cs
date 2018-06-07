﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Network;

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

        public byte PacketId { get; } = 9;

        public uint? ConstantLength => null;

        public IPacket New()
        {
            return new P09FileDataBlock();
        }

        public bool HandlePacket(PacketHandler handler)
        {
            return handler.HandleP09FileDataBlock(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            StartPosition = buf.ReadULong();
            DataBlock = buf.ReadByteArray(buf.Pending);
        }

        public void WritePacket(PacketBuffer buf)
        {
            buf.WriteULong(StartPosition);
            buf.WriteByteArray(DataBlock, false);
        }
    }
}