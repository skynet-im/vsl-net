using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.FileTransfer;
using VSL.Network;

namespace VSL.Packet
{
    internal class P07OpenFileTransfer : IPacket
    {
        internal Identifier Identifier { get; private set; }
        internal StreamMode StreamMode { get; private set; }

        internal P07OpenFileTransfer()
        {

        }

        internal P07OpenFileTransfer(Identifier identifier, StreamMode streamMode)
        {
            Identifier = identifier;
            StreamMode = streamMode;
        }

        public byte PacketId { get; } = 7;

        public uint? ConstantLength => null;

        public IPacket New() => new P07OpenFileTransfer();

        public bool HandlePacket(PacketHandler handler)
        {
            return handler.HandleP07OpenFileTransfer(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            Identifier = Identifier.FromBinary(buf);
            StreamMode = (StreamMode)buf.ReadByte();
            // reversing only incoming request to switch in the right perspective is done in the PacketHandler
        }

        public void WritePacket(PacketBuffer buf)
        {
            Identifier.ToBinary(buf);
            buf.WriteByte((byte)StreamMode);
        }

        public void ReverseStreamMode(ushort connectionVersion)
        {
            byte streamMode = (byte)StreamMode;
            if (connectionVersion < 2 && streamMode == 2)
                streamMode++; // StreamMode.PushHeader (zero based index 2) is not implemented in VSL 1.1
            StreamMode = (StreamMode)((streamMode + 2) % 4);
        }
    }
}