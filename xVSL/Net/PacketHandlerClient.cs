using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Net;
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
            RegisteredPackets = new List<PacketRule>
            {
                // P00Handshake     -   Server only
                // P01KeyExchange   -   Server only
                // P02Certificate   -   Not supported in VSL 1.1
                new PacketRule(new P03FinishHandshake(), CryptographicAlgorithm.None, CryptographicAlgorithm.Insecure_AES_256_CBC),
                new PacketRule(new P04ChangeIV(), CryptographicAlgorithm.Insecure_AES_256_CBC),
                // P05KeepAlive     -   Not supported in VSL 1.1
                new PacketRule(new P06Accepted(), CryptographicAlgorithm.Insecure_AES_256_CBC),
                // P07OpenFileTransfer - Server only
                new PacketRule(new P08FileHeader(), CryptographicAlgorithm.Insecure_AES_256_CBC),
                new PacketRule(new P09FileDataBlock(), CryptographicAlgorithm.Insecure_AES_256_CBC)
            };
        }
        //  constructor>

        // <functions
        internal override bool HandleP00Handshake(P00Handshake p)
        {
            parent.ExceptionHandler.CloseConnection("InvalidPacket", "VSL clients can not handle P00Handshake.");
            return false;
        }
        internal override bool HandleP01KeyExchange(P01KeyExchange p)
        {
            parent.ExceptionHandler.CloseConnection("InvalidPacket", "VSL clients can not handle P01KeyExchange.");
            return false;
        }
        internal override bool HandleP02Certificate(P02Certificate p)
        {
            throw new NotSupportedException("This VSL version does not support Certificates");
        }
        internal override bool HandleP03FinishHandshake(P03FinishHandshake p)
        {
            switch (p.ConnectionState)
            {
                case ConnectionState.CompatibilityMode:
                    parent.OnConnectionEstablished();
                    return true;
                case ConnectionState.Redirect:
                    parent.ExceptionHandler.CloseConnection("NotSupported", "This VSL version does not support redirects.");
                    return false;
                case ConnectionState.NotCompatible:
                    parent.ExceptionHandler.CloseConnection("ConnectionDenied", "The specified server denied the connection to this VSL/application version client.");
                    return false;
                case ConnectionState.Compatible:
                    parent.ConnectionVersion = p.VSLVersion;
                    parent.OnConnectionEstablished();
                    return true;
                default:
                    return false;
            }
        }
        internal override bool HandleP04ChangeIV(P04ChangeIV p)
        {
            parent.ExceptionHandler.CloseConnection("InvalidPacket", "VSL clients can not handle P04ChangeIV.");
            return false;
        }
        internal override bool HandleP05KeepAlive(P05KeepAlive p)
        {
            parent.ExceptionHandler.CloseConnection("InvalidPacket", "VSL clients can not handle P05KeepAlive.");
            return false;
        }
        // overriding void HandleP06Accepted is not neccessary.
        internal override bool HandleP07OpenFileTransfer(P07OpenFileTransfer p)
        {
            parent.ExceptionHandler.CloseConnection("InvalidPacket", "VSL clients can not handle P07OpenFileTransfer.");
            return false;
        }
        // overriding void HandleP08FileHeader is not neccessary.
        // overriding void HandleP09FileDataBlock is not neccessary.
        //  functions>
    }
}