using System;
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
        internal override void HandlePacket00Handshake(Packet00Handshake p)
        {
            switch (p.RequestType)
            {
                case Packet.RequestType.DirectPublicKey:
                    pending01KeyExchange = true;
                    break;
                default:
                    parent.SendPacket(new Packet03FinishHandshake(Packet.ConnectionType.NotCompatible));
                    break;
            }
        }
        internal override void HandlePacket01KeyExchange(Packet01KeyExchange p)
        {
            if (pending01KeyExchange)
            {
                //TODO: Implement Handler
            }
            else
            {
                parent.CloseConnection();
            }
        }
        internal override void HandlePacket02Certificate(Packet02Certificate p)
        {
            throw new NotImplementedException();
        }
        internal override void HandlePacket03FinishHandshake(Packet03FinishHandshake p)
        {
            //TODO: Implement Handler
        }
        internal override void HandlePacket04ChangeIV(Packet04ChangeIV p)
        {
            //TODO: Implement Handler
        }
        internal override void HandlePacket05KeepAlive(Packet05KeepAlive p)
        {
            //TODO: Implement Handler
        }
        //  functions>
    }
}