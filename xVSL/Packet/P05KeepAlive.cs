﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal class P05KeepAlive : IPacket
    {
        internal P05KeepAlive()
        {

        }

        public byte PacketID { get; } = 4;

        public uint? ConstantLength => 0;

        public IPacket New()
        {
            return new P05KeepAlive();
        }

        public bool HandlePacket(PacketHandler handler)
        {
            return handler.HandleP05KeepAlive(this);
        }

        public void ReadPacket(PacketBuffer buf)
        {
            
        }

        public void WritePacket(PacketBuffer buf)
        {
            
        }
    }
}