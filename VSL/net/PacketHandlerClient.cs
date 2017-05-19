using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

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
        internal override void HandlePacket00Handshake(P00Handshake p)
        {
            throw new NotImplementedException();
        }
        internal override void HandlePacket01KeyExchange(P01KeyExchange p)
        {
            throw new NotImplementedException();
        }
        internal override void HandlePacket02Certificate(P02Certificate p)
        {
            //TODO: Implement Handler
        }
        internal override void HandlePacket03FinishHandshake(P03FinishHandshake p)
        {
            //TODO: Implement Handler
        }
        internal override void HandlePacket04ChangeIV(P04ChangeIV p)
        {
            throw new NotImplementedException();
        }
        internal override void HandlePacket05KeepAlive(P05KeepAlive p)
        {
            throw new NotImplementedException();
        }
        //  functions>
    }
}