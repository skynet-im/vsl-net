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
        internal override async void HandleP00Handshake(P00Handshake p)
        {
            switch (p.RequestType)
            {
                case RequestType.DirectPublicKey:
                    break;
                default:
                    await parent.manager.SendPacketAsync(CryptographicAlgorithm.None, new P03FinishHandshake(ConnectionType.NotCompatible));
                    break;
            }
        }
        internal override async void HandleP01KeyExchange(P01KeyExchange p)
        {
            if (VersionManager.IsVSLVersionSupported(p.LatestVSL, p.OldestVSL) && VersionManager.IsProductVersionSupported(parent.LatestProduct, parent.OldestProduct, p.LatestProduct, p.OldestProduct))
            {
                parent.manager.AesKey = p.AesKey;
                parent.manager.SendIV = p.ServerIV;
                parent.manager.ReceiveIV = p.ClientIV;
                await parent.manager.SendPacketAsync(CryptographicAlgorithm.AES_256, new P03FinishHandshake(ConnectionType.Compatible));
            }
            else
                await parent.manager.SendPacketAsync(CryptographicAlgorithm.None, new P03FinishHandshake(ConnectionType.NotCompatible));
        }
        internal override void HandleP02Certificate(P02Certificate p)
        {
            throw new NotImplementedException();
        }
        internal override void HandleP03FinishHandshake(P03FinishHandshake p)
        {
            throw new NotSupportedException("This VSL version does not support unsafe fallback servers");
        }
        internal override void HandleP04ChangeIV(P04ChangeIV p)
        {
            parent.manager.SendIV = p.ServerIV;
            parent.manager.ReceiveIV = p.ClientIV;
        }
        internal override void HandleP05KeepAlive(P05KeepAlive p)
        {
            throw new NotSupportedException("This VSL version does not support keep alive packets");
        }
        internal override void HandleP06Accepted(P06Accepted p)
        {
            base.HandleP06Accepted(p);
        }
        internal override void HandleP07OpenFileTransfer(P07OpenFileTransfer p)
        {
            parent.FileTransfer.OnFileTransferRequested(p.Identifier, p.StreamMode);
        }
        internal override void HandleP08FileHeader(P08FileHeader p)
        {
            throw new NotImplementedException(); //
        }
        internal override void HandleP09FileDataBlock(P09FileDataBlock p)
        {
            if (parent.FileTransfer.ReceivingFile)
                parent.FileTransfer.OnDataBlockReceived(p);
            else
                throw new InvalidOperationException("No active file transfer");
        }
        //  functions>
    }
}