using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal class P06Accepted : IPacket
    {
        internal bool Accepted;
        internal byte RelatedPacket;
        internal ProblemCategory ProblemCategory;

        internal P06Accepted()
        {

        }

        internal P06Accepted(bool accepted, byte relatedPacket, ProblemCategory problemCategory)
        {
            Accepted = accepted;
            RelatedPacket = relatedPacket;
            ProblemCategory = problemCategory;
        }

        public byte ID { get; } = 6;

        public PacketLength PacketLength { get; } = new ConstantLength(3);

        public IPacket CreatePacket(byte[] buf)
        {
            P06Accepted packet = new P06Accepted();
            packet.ReadPacket(buf);
            return packet;
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandleP06Accepted(this);
        }

        public void ReadPacket(byte[] data)
        {
            PacketBuffer buf = new PacketBuffer(data);
            Accepted = buf.ReadBool();
            RelatedPacket = buf.ReadByte();
            ProblemCategory = (ProblemCategory)buf.ReadByte();
        }

        public byte[] WritePacket()
        {
            PacketBuffer buf = new PacketBuffer();
            buf.WriteBool(Accepted);
            buf.WriteByte(RelatedPacket);
            buf.WriteByte((byte)ProblemCategory);
            return buf.ToArray();
        }
    }
}