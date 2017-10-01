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

        public byte PacketID { get; } = 6;

        public PacketLength PacketLength { get; } = new ConstantLength(3);

        public IPacket New()
        {
            return new P06Accepted();
        }

        public bool HandlePacket(PacketHandler handler)
        {
            return handler.HandleP06Accepted(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            Accepted = buf.ReadBool();
            RelatedPacket = buf.ReadByte();
            ProblemCategory = (ProblemCategory)buf.ReadByte();
        }

        public void WritePacket(PacketBuffer buf)
        {
            buf.WriteBool(Accepted);
            buf.WriteByte(RelatedPacket);
            buf.WriteByte((byte)ProblemCategory);
        }
    }
}