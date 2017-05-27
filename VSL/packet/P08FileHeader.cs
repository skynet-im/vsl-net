using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal class P08FileHeader : IPacket
    {
        internal string Name;
        internal ulong Length;
        internal uint Attributes;
        internal DateTime CreationTime;
        internal DateTime LastAccessTime;
        internal DateTime LastWriteTime;
        internal byte[] Thumbnail;
        internal byte[] SHA256;

        internal P08FileHeader()
        {

        }

        public P08FileHeader(string name, ulong length, uint attributes, DateTime creationTime, DateTime lastAccessTime, DateTime lastWriteTime, byte[] thumbnail, byte[] sha256)
        {
            Name = name;
            Length = length;
            Attributes = attributes;
            CreationTime = creationTime;
            LastAccessTime = lastAccessTime;
            LastWriteTime = lastWriteTime;
            Thumbnail = thumbnail;
            SHA256 = sha256;
        }

        public byte ID { get; } = 8;

        public PacketLength PacketLength { get; } = new VariableLength();

        public IPacket CreatePacket(byte[] buf)
        {
            P08FileHeader packet = new P08FileHeader();
            packet.ReadPacket(buf);
            return packet;
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandleP08FileHeader(this);
        }

        public void ReadPacket(byte[] data)
        {
            PacketBuffer buf = new PacketBuffer(data);
            Name = buf.ReadString();
            Length = buf.ReadULong();
            Attributes = buf.ReadUInt();
            CreationTime = buf.ReadDate();
            LastAccessTime = buf.ReadDate();
            LastWriteTime = buf.ReadDate();
            Thumbnail = buf.ReadByteArray();
            SHA256 = buf.ReadByteArray(32);
        }

        public byte[] WritePacket()
        {
            PacketBuffer buf = new PacketBuffer();
            buf.WriteString(Name);
            buf.WriteULong(Length);
            buf.WriteUInt(Attributes);
            buf.WriteDate(CreationTime);
            buf.WriteDate(LastAccessTime);
            buf.WriteDate(LastWriteTime);
            buf.WriteByteArray(Thumbnail);
            buf.WriteByteArray(SHA256, false);
            return buf.ToArray();
        }
    }
}