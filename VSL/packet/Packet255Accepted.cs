using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class Packet255Accepted : IPacket
    {
        public bool Accepted;
        public string Reason = "";
        public Packet255Accepted()
        {

        }

        public byte ID
        {
            get
            {
                return 255;
            }
        }

        public void ReadPacket(byte[] buf)
        {
            PacketBuffer buffer = new PacketBuffer(buf);
            Accepted = buffer.ReadBool();
            Reason = buffer.ReadString();
        }

        public IPacket CreatePacket(byte[] buf)
        {
            Packet255Accepted packet = new Packet255Accepted();
            packet.ReadPacket(buf);
            return packet;
        }

        public byte[] WritePacket()
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBool(Accepted);
            buffer.WriteString(Reason);
            return buffer.ToArray();
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandlePacket255Accepted(this);
        }
    }
}