using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal class P08FileHeader : IPacket
    {
        internal byte[] BinaryData { get; private set; }

        internal P08FileHeader() { }

        public P08FileHeader(byte[] binaryData)
        {
            BinaryData = binaryData;
        }

        public byte PacketId { get; } = 8;

        public uint? ConstantLength => null;

        public IPacket New()
        {
            return new P08FileHeader();
        }

        public bool HandlePacket(PacketHandler handler)
        {
            return handler.HandleP08FileHeader(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            BinaryData = buf.ReadByteArray(buf.Pending);
        }

        public void WritePacket(PacketBuffer buf)
        {
            buf.WriteByteArray(BinaryData, false);
        }
    }
}