using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Net;
using VSL.Packet;

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
            RegisteredPackets = new List<PacketRule>
            {
                new PacketRule(new P00Handshake(), CryptographicAlgorithm.None),
                new PacketRule(new P01KeyExchange(), CryptographicAlgorithm.RSA_2048_OAEP),
                // P02Certificate   -   Not supported in VSL 1.1
                // P03FinishHandshake - Client only
                new PacketRule(new P04ChangeIV(), CryptographicAlgorithm.Insecure_AES_256_CBC),
                // P05KeepAlive     -   Not supported in VSL 1.1
                new PacketRule(new P06Accepted(), CryptographicAlgorithm.Insecure_AES_256_CBC),
                new PacketRule(new P07OpenFileTransfer(), CryptographicAlgorithm.Insecure_AES_256_CBC),
                new PacketRule(new P08FileHeader(), CryptographicAlgorithm.Insecure_AES_256_CBC),
                new PacketRule(new P09FileDataBlock(), CryptographicAlgorithm.Insecure_AES_256_CBC)
            };
        }
        //  constructor>

        // <functions
        internal override bool HandleP00Handshake(P00Handshake p)
        {
            switch (p.RequestType)
            {
                case RequestType.DirectPublicKey:
                    return true;
                default:
                    parent.manager.SendPacket(CryptographicAlgorithm.None, new P03FinishHandshake(ConnectionState.NotCompatible));
                    return true;
            }
        }
        internal override bool HandleP01KeyExchange(P01KeyExchange p)
        {
            ushort? vslVersion = VersionManager.GetSharedVSLVersion(p.LatestVSL, p.OldestVSL);
            ushort? productVersion = VersionManager.GetSharedProductVersion(parent.LatestProduct, parent.OldestProduct, p.LatestProduct, p.OldestProduct);

            if (!vslVersion.HasValue || !productVersion.HasValue)
                return parent.manager.SendPacket(CryptographicAlgorithm.None, new P03FinishHandshake(ConnectionState.NotCompatible));

            parent.manager.AesKey = p.AesKey;
            parent.manager.SendIV = p.ServerIV;
            parent.manager.ReceiveIV = p.ClientIV;
            parent.manager.Ready4Aes = true;
            parent.ConnectionVersion = vslVersion.Value;

            if (vslVersion.Value < 2)
            {
                if (!parent.manager.SendPacket(CryptographicAlgorithm.Insecure_AES_256_CBC, new P03FinishHandshake(ConnectionState.CompatibilityMode)))
                    return false;
                parent.OnConnectionEstablished();
            }

            if (vslVersion.Value == 2)
            {
                if (!parent.manager.SendPacket(CryptographicAlgorithm.AES_256_CBC_MP2, new P03FinishHandshake(ConnectionState.Compatible, vslVersion.Value, productVersion.Value)))
                    return false;
                parent.OnConnectionEstablished();
            }
            return true;
        }
        internal override bool HandleP02Certificate(P02Certificate p)
        {
            parent.ExceptionHandler.CloseConnection("InvalidPacket", "VSL servers can not handle P02Certificate.");
            return false;
        }
        internal override bool HandleP03FinishHandshake(P03FinishHandshake p)
        {
            parent.ExceptionHandler.CloseConnection("InvalidPacket", "VSL servers can not handle P03FinishHandshake.");
            return false;
        }
        internal override bool HandleP04ChangeIV(P04ChangeIV p)
        {
            parent.manager.SendIV = p.ServerIV;
            parent.manager.ReceiveIV = p.ClientIV;
            return true;
        }
        internal override bool HandleP05KeepAlive(P05KeepAlive p)
        {
            parent.ExceptionHandler.CloseConnection("NotSupported", "This VSL version does not support keep alive packets");
            return false;
        }
        // overriding void HandleP06Accepted is not neccessary.
        internal override bool HandleP07OpenFileTransfer(P07OpenFileTransfer p)
        {
            parent.FileTransfer.OnFileTransferRequested(p.Identifier, p.StreamMode);
            return true;
        }
        // overriding void HandleP08FileHeader is not neccessary.
        // overriding void HandleP09FileDataBlock is not neccessary.
        //  functions>
    }
}