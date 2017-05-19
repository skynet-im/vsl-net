using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSL.Crypt;

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
        internal void InitializeComponent()
        {

        }
        //  constructor>
        // <functions
        internal async Task OnDataReceive()
        {
            try
            {
                CryptographicAlgorithm algorithm = (CryptographicAlgorithm)(await parent.channel.ReadAsync(1))[0];
                switch (algorithm)
                {
                    case CryptographicAlgorithm.None:
                        await ReceivePacket_Plaintext();
                        break;
                    case CryptographicAlgorithm.RSA_2048:
                        await ReceivePacket_RSA_2048();
                        break;
                    case CryptographicAlgorithm.AES_256:
                        await ReceivePacket_AES_256();
                        break;
                }
            }
            catch (InvalidCastException ex) //Enum Parse
            {
                parent.ExceptionHandler.HandleInvalidCastException(ex);
                return;
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.HandleReceiveTimeoutException(ex);
                return;
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
                    parent.handler.TryHandlePacket(id, await parent.channel.ReadAsync(length));
                }
                else
                {
                    parent.ExceptionHandler.HandleInvalidOperationException(new InvalidOperationException());
                    return;
                }
            }
            catch (ArgumentOutOfRangeException ex) //PacketBuffer
            {
                parent.ExceptionHandler.HandleArgumentOutOfRangeException(ex);
            }
            catch (NotImplementedException ex) //PacketHandler
            {
                parent.ExceptionHandler.HandleNotImplementedException(ex);
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.HandleReceiveTimeoutException(ex);
            }
        }
        internal async Task ReceivePacket_RSA_2048()
        {
            try
            {
                string keypair = Keypair;
                byte[] ciphertext = await parent.channel.ReadAsync(256);
                byte[] plaintext = await Task.Run(() => RSA.DecryptBlock(ciphertext, keypair));
                byte id = plaintext.Take(1).ToArray()[0];
                plaintext = plaintext.Skip(1).ToArray();
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
                        length = BitConverter.ToUInt32(plaintext.Take(4).ToArray(), 0);
                        plaintext = plaintext.Skip(4).ToArray();
                    }
                    parent.handler.TryHandlePacket(id, plaintext.Take(Convert.ToInt32(length)).ToArray());
                }
                else
                {
                    parent.ExceptionHandler.HandleInvalidOperationException(new InvalidOperationException());
                    return;
                }
            }
            catch (ArgumentOutOfRangeException ex) //PacketBuffer
            {
                parent.ExceptionHandler.HandleArgumentOutOfRangeException(ex);
                return;
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                parent.ExceptionHandler.HandleCryptographicException(ex);
                return;
            }
            catch (NotImplementedException ex) //Keys, PacketHandler
            {
                parent.ExceptionHandler.HandleNotImplementedException(ex);
                return;
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.HandleReceiveTimeoutException(ex);
                return;
            }
        }
        internal async Task ReceivePacket_AES_256()
        {
            try
            {
                byte[] ciphertext = await parent.channel.ReadAsync(16); //TimeoutException
                byte[] plaintext = await AES.DecryptAsync(ciphertext, AesKey, ReceiveIV); //CryptographicException
                byte id = plaintext.Take(1).ToArray()[0];
                plaintext.Skip(1).ToArray();
                Packet.IPacket packet;
                bool success = parent.handler.TryGetPacket(id, out packet);
                uint length = 0;
                if (success && packet.Length.Type == Packet.PacketLength.LengthType.Constant)
                {
                    length = packet.Length.Length;
                }
                else
                {
                    length = BitConverter.ToUInt32(plaintext.Take(4).ToArray(), 0);
                    plaintext = plaintext.Skip(4).ToArray();
                }
                if (length > plaintext.Length - 3) // 3 random bytes in the header for more security
                {
                    uint pendingLength = Convert.ToUInt32(length - plaintext.Length - 3);
                    uint pendingPackets = Convert.ToUInt32(Math.Ceiling(pendingLength / 16d)); // round up
                    ciphertext = new byte[0];
                    for (int i = 0; i < pendingPackets; i++)
                    {
                        ciphertext = ciphertext.Concat(await parent.channel.ReadAsync(16)).ToArray(); //TimeoutException
                    }
                    plaintext = plaintext.Concat(await AES.DecryptAsync(ciphertext, AesKey, ReceiveIV)).ToArray();
                }
                int startIndex = Convert.ToInt32(plaintext.Length - length);
                byte[] content = plaintext.Skip(startIndex).ToArray(); // remove random bytes
                if (success)
                {
                    parent.handler.TryHandlePacket(id, content);
                }
                else
                {
                    parent.OnPacketReceived(id, content);
                }
            }
            catch (ArgumentOutOfRangeException ex) //PacketBuffer
            {
                parent.ExceptionHandler.HandleArgumentOutOfRangeException(ex);
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                parent.ExceptionHandler.HandleCryptographicException(ex);
            }
            catch (NotImplementedException ex) //PacketHandler
            {
                parent.ExceptionHandler.HandleNotImplementedException(ex);
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.HandleReceiveTimeoutException(ex);
            }
        }
        internal abstract string Keypair { get; }
        internal abstract byte[] AesKey { get; }
        internal abstract byte[] ReceiveIV { get; }
        //  functions>
    }
}