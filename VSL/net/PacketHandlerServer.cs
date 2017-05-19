using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

namespace VSL
{
    internal class PacketHandlerServer : PacketHandler
    {
        // <fields
        new internal VSLServer parent;
        private bool pending01KeyExchange = false;
        //  fields>

        // <constructor
        internal PacketHandlerServer(VSLServer parent)
        {
            this.parent = parent;
            base.parent = parent;
        }
        //  constructor>

        // <functions
        internal override void HandlePacket00Handshake(P00Handshake p)
        {
            switch (p.RequestType)
            {
                case RequestType.DirectPublicKey:
                    pending01KeyExchange = true;
                    break;
                default:
                    parent.SendPacket(new P03FinishHandshake(ConnectionType.NotCompatible));
                    break;
            }
        }
        internal override void HandlePacket01KeyExchange(P01KeyExchange p)
        {
            if (pending01KeyExchange)
            {
                //TODO: Implement Handler
            }
            else
            {
                parent.CloseConnection("Unexpected packet received");
            }
        }
        internal override void HandlePacket02Certificate(P02Certificate p)
        {
            throw new NotImplementedException();
        }
        internal override void HandlePacket03FinishHandshake(P03FinishHandshake p)
        {
            //TODO: Implement Handler
        }
        internal override void HandlePacket04ChangeIV(P04ChangeIV p)
        {
            //TODO: Implement Handler
        }
        internal override void HandlePacket05KeepAlive(P05KeepAlive p)
        {
            //TODO: Implement Handler
        }
        //  functions>
    }
}