using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSL
{
    /// <summary>
    /// Responsible for cryptography management
    /// </summary>
    internal abstract class NetworkManager
    {
        // <fields
        internal VSLSocket parent;
        //  fields>
        // <constructor
        //  constructor>
        // <functions
        internal async Task OnDataReceive()
        {
            try
            {
                CryptographicAlgorithm algorithm;
                try
                {
                    algorithm = (CryptographicAlgorithm)(await parent.channel.ReadAsync(1))[0];
                }
                catch (InvalidCastException ex)
                {
                    Console.WriteLine("[VSL] Cryptographic algorithm not supported: " + ex.ToString());
                    return;
                }
                switch (algorithm)
                {
                    case CryptographicAlgorithm.None:
                        await ReceivePacket_Plaintext();
                        break;
                    case CryptographicAlgorithm.RSA_2048:
                        break;
                    case CryptographicAlgorithm.AES_256:
                        break;
                }
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.HandleReceiveTimeoutException(ex);
            }
        }
        internal async Task ReceivePacket_Plaintext()
        {
            try
            {
                byte id = (await parent.channel.ReadAsync(1))[0];
                Packet.IPacket packet;
                bool success = parent.handler.TryGetPacket(id, out packet);
                if (success)
                {
                    uint length = 0;
                    if (packet.Length.Type == Packet.PacketLength.LengthType.Constant)
                    {
                        length = packet.Length.Length;
                    }
                    else if (packet.Length.Type == Packet.PacketLength.LengthType.UInt32)
                    {
                        length = BitConverter.ToUInt32(await parent.channel.ReadAsync(4), 0);
                    }
                    byte[] content = await parent.channel.ReadAsync(length);
                    Packet.IPacket finishedPacket = packet.CreatePacket(content);
                    finishedPacket.HandlePacket(parent.handler);
                }
                else
                {
                    parent.ExceptionHandler.HandleInvalidOperationException(new InvalidOperationException());
                }
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.HandleReceiveTimeoutException(ex);
            }
        }
        internal abstract Task ReceivePacket_RSA_2048();
        internal abstract Task ReceivePacket_AES_256();
        //  functions>
    }
}