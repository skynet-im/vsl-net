using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

namespace VSL
{
    internal class Packet03FinishHandshake : IPacket
    {
        internal ConnectionType ConnectionType;
        internal string Address;
        internal ushort Port;

        internal Packet03FinishHandshake()
        {

        }

        internal Packet03FinishHandshake(ConnectionType connectionType, string address, ushort port)
        {
            ConnectionType = connectionType;
            Address = address;
            Port = port;
        }

        public byte ID
        {
            get
            {
                return 3;
            }
        }

        public IPacket CreatePacket(byte[] buf)
        {
            Packet03FinishHandshake packet = new Packet03FinishHandshake();
            packet.ReadPacket(buf);
            return packet;
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandlePacket03FinishHandshake(this);
        }

        public void ReadPacket(byte[] buf)
        {
            PacketBuffer reader = new PacketBuffer(buf);
            ConnectionType = (ConnectionType)reader.ReadByte();
            if (ConnectionType == ConnectionType.Redirect)
            {
                Address = reader.ReadString();
                Port = reader.ReadUShort();
            }
        }

        public byte[] WritePacket()
        {
            PacketBuffer writer = new PacketBuffer();
            writer.WriteByte((byte)ConnectionType);
            if (ConnectionType == ConnectionType.Redirect)
            {
                writer.WriteString(Address);
                writer.WriteUShort(Port);
            }
            return writer.ToArray();
        }
    }
}