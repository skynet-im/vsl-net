﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class Packet00Handshake : IPacket
    {
        internal Packet.RequestType RequestType;

        internal Packet00Handshake()
        {

        }

        internal Packet00Handshake(Packet.RequestType requestType)
        {
            RequestType = requestType;
        }

        public byte ID
        {
            get
            {
                return 0;
            }
        }

        public IPacket CreatePacket(byte[] buf)
        {
            Packet00Handshake packet = new Packet00Handshake();
            packet.ReadPacket(buf);
            return packet;
        }

        public void HandlePacket(PacketHandler handler)
        {
            handler.HandlePacket00Handshake(this);
        }

        public void ReadPacket(byte[] buf)
        {
            throw new NotImplementedException();
        }

        public byte[] WritePacket()
        {
            throw new NotImplementedException();
        }
    }
}