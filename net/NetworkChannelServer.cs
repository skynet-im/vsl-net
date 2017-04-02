using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace VSL
{
    internal class NetworkChannelServer : NetworkChannel
    {
        // <fields
        new private VSLListener parent;
        //  fields>

        // <constructor
        internal NetworkChannelServer(VSLListener parent)
        {
            this.parent = parent;
            base.parent = parent;
        }
        //  constructor>

        // <functions
        internal override async void OnDataReveive(CancellationToken ct)
        {
            if (parent.ConnectionAvailable)
            {
                base.OnDataReveive(ct);
            }
            else
            {
                try
                {
                    byte[] buf = await Read(256, ct);
                    buf = await Crypt.RSA.Decrypt(buf, parent.Keypair);
                    PacketBuffer pb = new PacketBuffer(buf);
                    byte[] aesKey = pb.ReadByteArray(32);
                    byte[] clientIV = pb.ReadByteArray(16);
                    byte[] serverIV = pb.ReadByteArray(16);
                    uint vslVersion = pb.ReadUInt();
                    uint clientVersion = pb.ReadUInt();
                    AesKey = aesKey;
                    ReceiveIV = clientIV;
                    SendIV = serverIV;
                    if (vslVersion != Constants.VSLVersionNumber)
                    {
                        parent.SendPacket(new Packet255Accepted() { Accepted = false, Reason = "Invalid VSL version" });
                        return;
                    }
                    if (clientVersion != parent.TargetVersion)
                    {
                        parent.SendPacket(new Packet255Accepted() { Accepted = false, Reason = "Invalied client version" });
                        return;
                    }
                    parent.SendPacket(new Packet255Accepted() { Accepted = true });
                    parent.ConnectionAvailable = true;
                }
                catch (TimeoutException ex)
                {
                    Console.WriteLine("[VSL] TimeoutException in NetworkChannelServer.OnDataReceive(): " + ex.ToString());
                }
                catch (System.Security.Cryptography.CryptographicException ex)
                {
                    Console.WriteLine("[VSL] CryptographicException in NetworkChannelServer.OnDataReceive(): " + ex.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[VSL] Unknown exception in NetworkChannelServer.OnDataReceive(): " + ex.ToString());
                }
            }
            //  functions>
        }
    }
}