using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Network;

namespace VSL.Packet
{
    internal class P01KeyExchange : IPacket
    {
        internal byte[] AesKey { get; private set; }
        internal byte[] ClientIV { get; private set; }
        internal byte[] ServerIV { get; private set; }
        internal ushort LatestVSL { get; private set; }
        internal ushort OldestVSL { get; private set; }
        internal ushort LatestProduct { get; private set; }
        internal ushort OldestProduct { get; private set; }

        internal P01KeyExchange()
        {
        }

        internal P01KeyExchange(byte[] aesKey, byte[] clientIV, byte[] serverIV, ushort latestVSL, ushort oldestVSL, ushort latestProduct, ushort oldestProduct)
        {
            AesKey = aesKey;
            ClientIV = clientIV;
            ServerIV = serverIV;
            LatestVSL = latestVSL;
            OldestVSL = oldestVSL;
            LatestProduct = latestProduct;
            OldestProduct = oldestProduct;
        }

        public byte PacketId { get; } = 1;

        public uint? ConstantLength => 72;

        public IPacket New() => new P01KeyExchange();

        public Task<bool> HandlePacketAsync(PacketHandler handler)
        {
            return handler.HandleP01KeyExchange(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            AesKey = buf.ReadByteArray(32);
            ClientIV = buf.ReadByteArray(16);
            ServerIV = buf.ReadByteArray(16);
            LatestVSL = buf.ReadUShort();
            OldestVSL = buf.ReadUShort();
            LatestProduct = buf.ReadUShort();
            OldestProduct = buf.ReadUShort();
        }

        public void WritePacket(PacketBuffer buf)
        {
            buf.WriteByteArray(AesKey, false);
            buf.WriteByteArray(ClientIV, false);
            buf.WriteByteArray(ServerIV, false);
            buf.WriteUShort(LatestVSL);
            buf.WriteUShort(OldestVSL);
            buf.WriteUShort(LatestProduct);
            buf.WriteUShort(OldestProduct);
        }
    }
}