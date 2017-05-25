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
        internal override async void HandlePacket00Handshake(P00Handshake p)
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
        internal override async void HandlePacket01KeyExchange(P01KeyExchange p)
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
        internal override void HandlePacket02Certificate(P02Certificate p)
        {
            throw new NotImplementedException();
        }
        internal override void HandlePacket03FinishHandshake(P03FinishHandshake p)
        {
            throw new NotSupportedException("This VSL version does not support unsafe fallback servers");
        }
        internal override void HandlePacket04ChangeIV(P04ChangeIV p)
        {
            parent.manager.SendIV = p.ServerIV;
            parent.manager.ReceiveIV = p.ClientIV;
        }
        internal override void HandlePacket05KeepAlive(P05KeepAlive p)
        {
            throw new NotSupportedException("This VSL version does not support keep alive packets");
        }
        //  functions>
    }
}