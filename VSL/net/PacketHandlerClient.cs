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
            InitializeComponent();
        }
        //  constructor>

        // <functions
        internal override void HandleP00Handshake(P00Handshake p)
        {
            throw new NotImplementedException();
        }
        internal override void HandleP01KeyExchange(P01KeyExchange p)
        {
            throw new NotImplementedException();
        }
        internal override void HandleP02Certificate(P02Certificate p)
        {
            throw new NotSupportedException("This VSL version does not support Certificates");
        }
        internal override void HandleP03FinishHandshake(P03FinishHandshake p)
        {
            parent.Logger.d("handling packet 03: " + p.ConnectionType.ToString());
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
        internal override void HandleP04ChangeIV(P04ChangeIV p)
        {
            throw new NotImplementedException();
        }
        internal override void HandleP05KeepAlive(P05KeepAlive p)
        {
            throw new NotImplementedException();
        }
        internal override void HandleP06Accepted(P06Accepted p)
        {
            throw new NotImplementedException();
        }
        internal override void HandleP07OpenFileTransfer(P07OpenFileTransfer p)
        {
            throw new NotImplementedException();
        }
        internal override void HandleP08FileHeader(P08FileHeader p)
        {
            throw new NotImplementedException();
        }
        internal override void HandleP09FileDataBlock(P09FileDataBlock p)
        {
            throw new NotImplementedException();
        }
        //  functions>
    }
}