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
            throw new NotSupportedException("This VSL version does not support Certificates");
        }
        internal override void HandlePacket03FinishHandshake(P03FinishHandshake p)
        {
            switch (p.ConnectionType)
            {
                case ConnectionType.Compatible:
                    parent.OnConnectionEstablished();
                    break;
                case ConnectionType.Redirect:
                    throw new NotSupportedException("This VSL version does not support redirects");
                case ConnectionType.NotCompatible:
                    parent.CloseConnection("The specified server does not support this VSL/application version");
                    break;
            }
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