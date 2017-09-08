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
        //  fields>

        // <constructor
        internal PacketHandlerServer(VSLServer parent)
        {
            this.parent = parent;
            base.parent = parent;
            InitializeComponent();
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
                    parent.manager.SendPacket(CryptographicAlgorithm.None, new P03FinishHandshake(ConnectionType.NotCompatible));
                    return true;
            }
        }
        internal override bool HandleP01KeyExchange(P01KeyExchange p)
        {
            if (VersionManager.IsVSLVersionSupported(p.LatestVSL, p.OldestVSL) && VersionManager.IsProductVersionSupported(parent.LatestProduct, parent.OldestProduct, p.LatestProduct, p.OldestProduct))
            {
                parent.manager.AesKey = p.AesKey;
                parent.manager.SendIV = p.ServerIV;
                parent.manager.ReceiveIV = p.ClientIV;
                parent.manager.Ready4Aes = true;
                parent.manager.SendPacket(CryptographicAlgorithm.AES_256, new P03FinishHandshake(ConnectionType.Compatible));
                parent.OnConnectionEstablished();
            }
            else
                parent.manager.SendPacket(CryptographicAlgorithm.None, new P03FinishHandshake(ConnectionType.NotCompatible));
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