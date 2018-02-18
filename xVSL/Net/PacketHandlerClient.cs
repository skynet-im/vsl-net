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
                new PacketRule(new P03FinishHandshake(), CryptoAlgorithm.None, CryptoAlgorithm.AES_256_CBC_SP, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3),
                new PacketRule(new P04ChangeIV(), CryptoAlgorithm.AES_256_CBC_SP, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3),
                // P05KeepAlive     -   Not supported in VSL 1.1
                new PacketRule(new P06Accepted(), CryptoAlgorithm.AES_256_CBC_SP, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3),
                new PacketRule(new P07OpenFileTransfer(), CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3), // since VSL 1.2
                new PacketRule(new P08FileHeader(), CryptoAlgorithm.AES_256_CBC_SP, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3),
                new PacketRule(new P09FileDataBlock(), CryptoAlgorithm.AES_256_CBC_SP, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3)
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
                    parent.ConnectionVersion = 1; // VSL 1.1 is the last version that uses this mode.
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
        // overriding HandleP05KeepAlive is not neccessary.
        // overriding HandleP06Accepted is not neccessary.
        internal override bool HandleP07OpenFileTransfer(P07OpenFileTransfer p)
        {
            if (parent.ConnectionVersion.Value < 2) // before VSL 1.2
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket", "VSL clients can not handle P07OpenFileTransfer.");
                return false;
            }
            else
            {
                return parent.FileTransfer.OnPacketReceived(p);
            }
        }
        // overriding HandleP08FileHeader is not neccessary.
        // overriding HandleP09FileDataBlock is not neccessary.
        //  functions>
    }
}