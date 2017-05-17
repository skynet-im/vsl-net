using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

namespace VSL.Packet
{
    internal class P03FinishHandshake : IPacket
    {
        internal ConnectionType ConnectionType;
        internal string Address;
        internal ushort Port;

        internal P03FinishHandshake()
        {

        }

        internal P03FinishHandshake(ConnectionType connectionType, string address = null, ushort port = 0)
        {
            ConnectionType = connectionType;
            Address = address;
            Port = port;
        }

        public byte ID { get; } = 3;

        public PacketLength Length { get; } = new VariableLength();
        
        public IPacket CreatePacket(byte[] buf)
        {
            P03FinishHandshake packet = new P03FinishHandshake();
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