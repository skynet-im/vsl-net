using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VSL.BinaryTools;
using VSL.Packet;

namespace VSL.Network
{
    internal class PacketHandlerServer : PacketHandler
    {
        // <fields
        private static readonly PacketRule[] rules;
        private readonly ushort latestProduct;
        private readonly ushort oldestProduct;
        //  fields>

        // <constructor
        static PacketHandlerServer()
        {
            rules = InitRules(
                new PacketRule(new P00Handshake(), CryptoAlgorithm.None),
                new PacketRule(new P01KeyExchange(), CryptoAlgorithm.RSA_2048_OAEP),
                // P02Certificate   -   Not supported in VSL 1.1
                // P03FinishHandshake - Client only
                new PacketRule(new P04ChangeIV(), CryptoAlgorithm.AES_256_CBC_SP, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3),
                new PacketRule(new P05KeepAlive(), CryptoAlgorithm.None),  // since VSL 1.2.2
                new PacketRule(new P06Accepted(), CryptoAlgorithm.AES_256_CBC_SP, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3),
                new PacketRule(new P07OpenFileTransfer(), CryptoAlgorithm.AES_256_CBC_SP, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3),
                new PacketRule(new P08FileHeader(), CryptoAlgorithm.AES_256_CBC_SP, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3),
                new PacketRule(new P09FileDataBlock(), CryptoAlgorithm.AES_256_CBC_SP, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3)
            );
        }
        internal PacketHandlerServer(VSLServer parent, ushort latestProduct, ushort oldestProduct) : base(parent)
        {
            this.latestProduct = latestProduct;
            this.oldestProduct = oldestProduct;
        }
        //  constructor>

        // <properties
        protected override PacketRule[] RegisteredPackets => rules;
        //  properties>

        // <functions
        internal override Task<bool> HandleP00Handshake(P00Handshake p)
        {
            switch (p.RequestType)
            {
                case RequestType.DirectPublicKey:
                    return Task.FromResult(true);
                default:
                    return parent.Manager.SendPacketAsync(CryptoAlgorithm.None, new P03FinishHandshake(ConnectionState.NotCompatible));
            }
        }
        internal override async Task<bool> HandleP01KeyExchange(P01KeyExchange p)
        {
            ushort? vslVersion = VersionManager.GetSharedVSLVersion(p.LatestVSL, p.OldestVSL);
            ushort? productVersion = VersionManager.GetSharedProductVersion(latestProduct, oldestProduct, p.LatestProduct, p.OldestProduct);

            if (!vslVersion.HasValue || !productVersion.HasValue)
                return await parent.Manager.SendPacketAsync(CryptoAlgorithm.None, new P03FinishHandshake(ConnectionState.NotCompatible));

            parent.Manager.AesKey = p.AesKey;
            parent.ConnectionVersion = vslVersion.Value;

            if (vslVersion.Value < 2)
            {
                parent.Manager.SendIV = p.ServerIV;
                parent.Manager.ReceiveIV = p.ClientIV;
                parent.Manager.Ready4Aes = true;

                if (!await parent.Manager.SendPacketAsync(CryptoAlgorithm.AES_256_CBC_SP, new P03FinishHandshake(ConnectionState.CompatibilityMode)))
                    return false;
                parent.OnConnectionEstablished();
            }

            if (vslVersion.Value == 2)
            {
                parent.Manager.HmacKey = Util.ConcatBytes(p.ClientIV, p.ServerIV);
                parent.Manager.Ready4Aes = true;

                if (!await parent.Manager.SendPacketAsync(CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3, new P03FinishHandshake(ConnectionState.Compatible, vslVersion.Value, productVersion.Value)))
                    return false;
                parent.OnConnectionEstablished();
            }
            return true;
        }
        internal override Task<bool> HandleP02Certificate(P02Certificate p)
        {
            parent.ExceptionHandler.CloseConnection("InvalidPacket", "VSL servers can not handle P02Certificate.");
            return Task.FromResult(false);
        }
        internal override Task<bool> HandleP03FinishHandshake(P03FinishHandshake p)
        {
            parent.ExceptionHandler.CloseConnection("InvalidPacket", "VSL servers can not handle P03FinishHandshake.");
            return Task.FromResult(false);
        }
        internal override Task<bool> HandleP04ChangeIV(P04ChangeIV p)
        {
            if (parent.ConnectionVersion.Value > 1)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "P04ChangeIV is not supported in VSL 1.2 because ivs are generated for each packet.\r\n" +
                    "\tat PacketHandlerServer.HandleP04ChangeIV(P04ChangeIV)");
                return Task.FromResult(false);
            }
            else
            {
                parent.Manager.SendIV = p.ServerIV;
                parent.Manager.ReceiveIV = p.ClientIV;
                return Task.FromResult(true);
            }
        }
        // overriding HandleP05KeepAlive is not neccessary.
        // overriding HandleP06Accepted is not neccessary.
        // overriding HandleP07OpenFileTransfer is not neccessary.
        // overriding HandleP08FileHeader is not neccessary.
        // overriding HandleP09FileDataBlock is not neccessary.
        //  functions>
    }
}