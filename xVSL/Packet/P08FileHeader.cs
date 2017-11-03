using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal class P08FileHeader : IPacket
    {
        internal string Name;
        internal ulong Length;
        internal FileAttributes Attributes;
        internal DateTime CreationTime;
        internal DateTime LastAccessTime;
        internal DateTime LastWriteTime;
        internal byte[] Thumbnail;
        internal byte[] SHA256;

        internal P08FileHeader()
        {

        }

        public P08FileHeader(string name, ulong length, FileAttributes attributes, DateTime creationTime, DateTime lastAccessTime, DateTime lastWriteTime, byte[] thumbnail, byte[] sha256)
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

        public byte PacketID { get; } = 8;

        public uint? ConstantLength => null;

        public IPacket New()
        {
            return new P08FileHeader();
        }

        public bool HandlePacket(PacketHandler handler)
        {
            return handler.HandleP08FileHeader(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            Name = buf.ReadString();
            Length = buf.ReadULong();
            Attributes = (FileAttributes)buf.ReadUInt();
            CreationTime = buf.ReadDate();
            LastAccessTime = buf.ReadDate();
            LastWriteTime = buf.ReadDate();
            Thumbnail = buf.ReadByteArray();
            SHA256 = buf.ReadByteArray(32);
        }

        public void WritePacket(PacketBuffer buf)
        {
            buf.WriteString(Name);
            buf.WriteULong(Length);
            buf.WriteUInt((uint)Attributes);
            buf.WriteDate(CreationTime);
            buf.WriteDate(LastAccessTime);
            buf.WriteDate(LastWriteTime);
            buf.WriteByteArray(Thumbnail);
            buf.WriteByteArray(SHA256, false);
        }
    }
}