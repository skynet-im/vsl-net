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
        internal async Task OnDataReceiveAsync()
        {
            try
            {
                CryptographicAlgorithm algorithm = (CryptographicAlgorithm)(await parent.channel.ReadAsync(1))[0];
                switch (algorithm)
                {
                    case CryptographicAlgorithm.None:
                        await ReceivePacketAsync_Plaintext();
                        break;
                    case CryptographicAlgorithm.RSA_2048:
                        await ReceivePacketAsync_RSA_2048();
                        break;
                    case CryptographicAlgorithm.AES_256:
                        await ReceivePacketAsync_AES_256();
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
        private async Task ReceivePacketAsync_Plaintext()
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
        private async Task ReceivePacketAsync_RSA_2048()
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
        private async Task ReceivePacketAsync_AES_256()
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
                    uint pendingLength = Convert.ToUInt32(length - plaintext.Length + 3);
                    uint pendingBlocks = Convert.ToUInt32(Math.Ceiling(pendingLength / 16d)); // round up
                    ciphertext = await parent.channel.ReadAsync(pendingBlocks * 16);
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
        internal Task SendPacketAsync(byte id, byte[] content)
        {
            byte[] head = new byte[1] { id };
            head = head.Concat(BitConverter.GetBytes(Convert.ToUInt32(content.Length))).ToArray();
            return SendPacketAsync(CryptographicAlgorithm.AES_256, head, content);
        }
        internal Task SendPacketAsync(CryptographicAlgorithm alg, Packet.IPacket packet)
        {
            byte[] head = new byte[1] { packet.ID };
            byte[] content = packet.WritePacket();
            if (packet.Length.Type == Packet.PacketLength.LengthType.UInt32)
                head = head.Concat(BitConverter.GetBytes(Convert.ToUInt32(content.Length))).ToArray();
            return SendPacketAsync(alg, head, content);
        }
        internal async Task SendPacketAsync(CryptographicAlgorithm alg, byte[] head, byte[] content)
        {
            switch (alg)
            {
                case CryptographicAlgorithm.None:
                    SendPacket_Plaintext(head, content);
                    break;
                case CryptographicAlgorithm.RSA_2048:
                    await SendPacketAsync_RSA_2048(head, content);
                    break;
                case CryptographicAlgorithm.AES_256:
                    await SendPacketAsync_AES_256(head, content);
                    break;
            }
        }
        private void SendPacket_Plaintext(byte[] head, byte[] content)
        {
            parent.channel.SendAsync(Util.ConnectBytesPA(new byte[1] { (byte)CryptographicAlgorithm.None }, head, content));
        }
        private async Task SendPacketAsync_RSA_2048(byte[] head, byte[] content)
        {
            try
            {
                byte[] ciphertext = await RSA.EncryptAsync(Util.ConnectBytesPA(head, content), PublicKey);
                parent.channel.SendAsync(Util.ConnectBytesPA(new byte[1] { (byte)CryptographicAlgorithm.RSA_2048 }, ciphertext));
            }
            catch (System.Security.Cryptography.CryptographicException ex) //Invalid key
            {
                parent.ExceptionHandler.HandleCryptographicException(ex);
            }
            catch (NotImplementedException ex) //Keys
            {
                parent.ExceptionHandler.HandleNotImplementedException(ex);
            }
        }
        private async Task SendPacketAsync_AES_256(byte[] head, byte[] content)
        {
            try
            {
                uint blocks = Convert.ToUInt32(Math.Ceiling((head.Length + 3 + head.Length) / 16d)); //at least 3 random bytes in the header block
                byte[] salt = new byte[blocks * 16 - head.Length - content.Length]; //calculate number of random bytes
                Random random = new Random();
                random.NextBytes(salt);
                byte[] plaintext = Util.ConnectBytesPA(head, salt, content);
                byte[] headBlock = await AES.EncryptAsync(plaintext.Take(16).ToArray(), AesKey, SendIV);
                byte[] tailBlock = new byte[0];
                if (plaintext.Length > 16)
                {
                    plaintext = plaintext.Skip(16).ToArray();
                    tailBlock = await AES.EncryptAsync(plaintext, AesKey, SendIV);
                }
                parent.channel.SendAsync(Util.ConnectBytesPA(new byte[1] { (byte)CryptographicAlgorithm.AES_256 }, headBlock, tailBlock));
            }
            catch (System.Security.Cryptography.CryptographicException ex) //Invalid key/iv
            {
                parent.ExceptionHandler.HandleCryptographicException(ex);
            }
        }
        internal abstract string PublicKey { get; }
        internal abstract string Keypair { get; }
        internal abstract byte[] AesKey { get; }
        internal abstract byte[] ReceiveIV { get; }
        internal abstract byte[] SendIV { get; }
        //  functions>
    }
}