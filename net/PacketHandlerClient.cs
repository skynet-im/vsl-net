using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class PacketHandlerClient : PacketHandler
    {
        // <fields
        new internal VSLClient parent;
        //  fields>

        // <constructor
        internal PacketHandlerClient(VSLClient parent)
        {
            this.parent = parent;
            base.parent = parent;
        }
        //  constructor>

        // <functions
        internal override void HandlePacket255Accepted(Packet255Accepted p)
        {
            if (p.Accepted) throw new NotImplementedException();
        }
        //  functions>
    }
}