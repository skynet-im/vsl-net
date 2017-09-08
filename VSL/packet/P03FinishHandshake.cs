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

        public byte PacketID { get; } = 3;

        public PacketLength PacketLength { get; } = new VariableLength();

        public IPacket New()
        {
            return new P03FinishHandshake();
        }

        public bool HandlePacket(PacketHandler handler)
        {
            return handler.HandleP03FinishHandshake(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            ConnectionType = (ConnectionType)buf.ReadByte();
            if (ConnectionType == ConnectionType.Redirect)
            {
                Address = buf.ReadString();
                Port = buf.ReadUShort();
            }
        }

        public void WritePacket(PacketBuffer buf)
        {
            buf.WriteByte((byte)ConnectionType);
            if (ConnectionType == ConnectionType.Redirect)
            {
                buf.WriteString(Address);
                buf.WriteUShort(Port);
            }
        }
    }
}