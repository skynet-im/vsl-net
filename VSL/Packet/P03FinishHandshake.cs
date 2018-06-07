using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Network;

namespace VSL.Packet
{
    internal class P03FinishHandshake : IPacket
    {
        internal ConnectionState ConnectionState { get; private set; }
        internal string Address { get; private set; }
        internal ushort Port { get; private set; }
        /// <summary>
        /// Gets the VSL protocol version used for this connection. This value is available since VSL 1.2.
        /// </summary>
        internal ushort VSLVersion { get; private set; }
        /// <summary>
        /// Gets the product protocol version used for this connection. This value is available since VSL 1.2.
        /// </summary>
        internal ushort ProductVersion { get; private set; }

        internal P03FinishHandshake()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="P03FinishHandshake"/> class for <see cref="ConnectionState.CompatibilityMode"/> or <see cref="ConnectionState.NotCompatible"/>.
        /// </summary>
        /// <param name="connectionState"></param>
        internal P03FinishHandshake(ConnectionState connectionState)
        {
            if (connectionState != ConnectionState.CompatibilityMode && connectionState != ConnectionState.NotCompatible)
                throw new ArgumentException("P03FinishHandshake.P03FinishHandshake(ConnectionState) is only allowed with ConnectionState.CompatibilityMode or onnectionState.NotCompatible", "connectionState");
            ConnectionState = connectionState;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="P03FinishHandshake"/> class for <see cref="ConnectionState.Redirect"/>.
        /// </summary>
        /// <param name="connectionState"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        internal P03FinishHandshake(ConnectionState connectionState, string address, ushort port)
        {
            ConnectionState = connectionState;
            Address = address;
            Port = port;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="P03FinishHandshake"/> class for <see cref="ConnectionState.Compatible"/>.
        /// </summary>
        /// <param name="connectionState"></param>
        /// <param name="vslVersion"></param>
        /// <param name="productVersion"></param>
        internal P03FinishHandshake(ConnectionState connectionState, ushort vslVersion, ushort productVersion)
        {
            ConnectionState = connectionState;
            VSLVersion = vslVersion;
            ProductVersion = productVersion;
        }

        public byte PacketId { get; } = 3;

        public uint? ConstantLength => null;

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
            ConnectionState = (ConnectionState)buf.ReadByte();
            if (ConnectionState == ConnectionState.Redirect)
            {
                Address = buf.ReadString();
                Port = buf.ReadUShort();
            }
            else if (ConnectionState == ConnectionState.Compatible)
            {
                VSLVersion = buf.ReadUShort();
                ProductVersion = buf.ReadUShort();
            }
        }

        public void WritePacket(PacketBuffer buf)
        {
            buf.WriteByte((byte)ConnectionState);
            if (ConnectionState == ConnectionState.Redirect)
            {
                buf.WriteString(Address);
                buf.WriteUShort(Port);
            }
            else if (ConnectionState == ConnectionState.Compatible)
            {
                buf.WriteUShort(VSLVersion);
                buf.WriteUShort(ProductVersion);
            }
        }
    }
}