using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

namespace VSL.Packet
{
    internal class P01KeyExchange : IPacket
    {
        internal byte[] AesKey;
        internal byte[] ClientIV;
        internal byte[] ServerIV;
        internal ushort LatestVSL;
        internal ushort OldestVSL;
        internal ushort LatestProduct;
        internal ushort OldestProduct;

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

        public byte ID { get; } = 1;

        public PacketLength Length { get; } = new ConstantLength(72);

        public IPacket CreatePacket(byte[] buf)
        {
            P01KeyExchange packet = new P01KeyExchange();
            packet.ReadPacket(buf);
            return packet;
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandlePacket01KeyExchange(this);
        }

        public void ReadPacket(byte[] buf)
        {
            PacketBuffer reader = new PacketBuffer(buf);
            AesKey = reader.ReadByteArray(32);
            ClientIV = reader.ReadByteArray(16);
            ServerIV = reader.ReadByteArray(16);
            LatestVSL = reader.ReadUShort();
            OldestVSL = reader.ReadUShort();
            LatestProduct = reader.ReadUShort();
            OldestProduct = reader.ReadUShort();
        }

        public byte[] WritePacket()
        {
            PacketBuffer writer = new PacketBuffer();
            writer.WriteByteArray(AesKey, false);
            writer.WriteByteArray(ClientIV, false);
            writer.WriteByteArray(ServerIV, false);
            writer.WriteUShort(LatestVSL);
            writer.WriteUShort(OldestVSL);
            writer.WriteUShort(LatestProduct);
            writer.WriteUShort(OldestProduct);
            return writer.ToArray();
        }
    }
}