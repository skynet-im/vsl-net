using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Network;

namespace VSL.Packet
{
    internal class P06Accepted : IPacket
    {
        internal bool Accepted { get; private set; }
        internal byte RelatedPacket { get; private set; }
        internal ProblemCategory ProblemCategory { get; private set; }

        internal P06Accepted()
        {
        }

        internal P06Accepted(bool accepted, byte relatedPacket, ProblemCategory problemCategory)
        {
            Accepted = accepted;
            RelatedPacket = relatedPacket;
            ProblemCategory = problemCategory;
        }

        public byte PacketId { get; } = 6;

        public uint? ConstantLength => 3;

        public IPacket New()
        {
            return new P06Accepted();
        }

        public Task<bool> HandlePacketAsync(PacketHandler handler)
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