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
        internal override void HandlePacket00Handshake(Packet00Handshake p)
        {
            throw new NotImplementedException();
        }
        internal override void HandlePacket01KeyExchange(Packet01KeyExchange p)
        {
            throw new NotImplementedException();
        }
        internal override void HandlePacket02Certificate(Packet02Certificate p)
        {
            //TODO: Implement Handler
        }
        internal override void HandlePacket03FinishHandshake(Packet03FinishHandshake p)
        {
            //TODO: Implement Handler
        }
        internal override void HandlePacket04ChangeIV(Packet04ChangeIV p)
        {
            throw new NotImplementedException();
        }
        //  functions>
    }
}