﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class PacketHandlerServer : PacketHandler
    {
        // <fields
        new internal VSLServer parent;
        //  fields>

        // <constructor
        internal PacketHandlerServer(VSLServer parent)
        {
            this.parent = parent;
            base.parent = parent;
        }
        //  constructor>

        // <functions
        internal override void HandlePacket255Accepted(Packet255Accepted p)
        {
            throw new NotImplementedException();
        }
        //  functions>
    }
}