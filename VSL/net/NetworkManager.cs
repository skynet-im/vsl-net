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
        private bool EnableReceive = true;
        //  fields>
        // <constructor
        internal void InitializeComponent()
        {

        }
        //  constructor>
        // <functions
        internal async void OnDataReceive()
        {
            if (!EnableReceive) return;
            try
            {
                byte b = (await parent.channel.ReadAsync(1))[0];
                CryptographicAlgorithm algorithm = (CryptographicAlgorithm)b;
                parent.Logger.d("Received packet with algorithm " + algorithm.ToString());
                switch (algorithm)
                {
                    case CryptographicAlgorithm.None:
                        await ReceivePacketAsync_Plaintext();
                        break;
                    case CryptographicAlgorithm.RSA_2048:
                        await ReceivePacketAsync_RSA_2048();
                        break;
                    case CryptographicAlgorithm.AES_256:
                        ReceivePacket_AES_256();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            catch (InvalidOperationException ex)
            {
                parent.ExceptionHandler.HandleInvalidOperationException(ex);
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
                    parent.handler.HandleInternalPacket(id, await parent.channel.ReadAsync(length));
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
            catch (InvalidCastException ex) //PacketHandler
            {
                parent.ExceptionHandler.HandleInvalidCastException(ex);
            }
            catch (NotImplementedException ex) //PacketHandler
            {
                parent.ExceptionHandler.HandleNotImplementedException(ex);
            }
            catch (NotSupportedException ex) //PacketHandler
            {
                parent.ExceptionHandler.HandleNotSupportedException(ex);
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
                    parent.handler.HandleInternalPacket(id, plaintext.Take(Convert.ToInt32(length)).ToArray());
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
                parent.ExceptionHandler.HandleException(ex);
                return;
            }
            catch (InvalidCastException ex) //PacketHandler
            {
                parent.ExceptionHandler.HandleInvalidCastException(ex);
            }
            catch (NotImplementedException ex) //PacketHandler
            {
                parent.ExceptionHandler.HandleNotImplementedException(ex);
            }
            catch (NotSupportedException ex) //PacketHandler
            {
                parent.ExceptionHandler.HandleNotSupportedException(ex);
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.HandleReceiveTimeoutException(ex);
                return;
            }
        }
        private async void ReceivePacket_AES_256()
        {
            //try
            //{
            byte[] ciphertext = await parent.channel.ReadAsync(16); //TimeoutException
            Console.WriteLine("receiving AES packet:" + Util.ToHexString(ciphertext));
            byte[] plaintext = await AES.DecryptAsync(ciphertext, AesKey, ReceiveIV); //CryptographicException
            Console.WriteLine("decrypted packet: " + Util.ToHexString(plaintext));
            byte id = plaintext.Take(1).ToArray()[0];
            plaintext = plaintext.Skip(1).ToArray();
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
            Console.WriteLine("AES Packet length=" + length);
            if (length > plaintext.Length - 2) // 2 random bytes in the header for more security
            {
                EnableReceive = false;
                int pendingLength = Convert.ToInt32(length - plaintext.Length + 2);
                Console.WriteLine("AES Packet pending length=" + pendingLength);
                int pendingBlocks = Convert.ToInt32(Math.Ceiling((pendingLength + 1) / 16d)); // round up, first blocks only 15 bytes (padding)
                Console.WriteLine("AES Packet pending blocks=" + pendingBlocks);
                ciphertext = await parent.channel.ReadAsync(pendingBlocks * 16);
                Console.WriteLine("AES Packet next ciphertext =" + Util.ToHexString(ciphertext));
                plaintext = plaintext.Concat(await AES.DecryptAsync(ciphertext, AesKey, ReceiveIV)).ToArray();
            }
            int startIndex = Convert.ToInt32(plaintext.Length - length);
            byte[] content = plaintext.Skip(startIndex).ToArray(); // remove random bytes
            if (success)
            {
                parent.handler.HandleInternalPacket(id, content);
            }
            else
            {
                parent.OnPacketReceived(id, content);
            }
            //}
            /*catch (ArgumentOutOfRangeException ex) //PacketBuffer
            {
                parent.ExceptionHandler.HandleArgumentOutOfRangeException(ex);
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                parent.ExceptionHandler.HandleException(ex);
            }
            catch (InvalidCastException ex) //PacketHandler
            {
                parent.ExceptionHandler.HandleInvalidCastException(ex);
            }
            catch (NotImplementedException ex) //PacketHandler
            {
                parent.ExceptionHandler.HandleNotImplementedException(ex);
            }
            catch (NotSupportedException ex) //PacketHandler
            {
                parent.ExceptionHandler.HandleNotSupportedException(ex);
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.HandleReceiveTimeoutException(ex);
            }*/
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
            Console.WriteLine("sending packet with length " + content.Length);
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
                parent.ExceptionHandler.HandleException(ex);
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
                int blocks;
                int saltLength;
                if (head.Length + 2 + content.Length < 16)
                {
                    blocks = 1;
                    saltLength = 15 - head.Length - content.Length;
                }
                else
                {
                    blocks = Convert.ToInt32(Math.Ceiling((head.Length + 4 + content.Length) / 16d)); //at least 2 random bytes in the header block
                    saltLength = blocks * 16 - head.Length - content.Length - 2; //first blocks only 15 bytes (padding)
                }
                Console.WriteLine("salt length: " + Convert.ToString(saltLength));
                byte[] salt = new byte[saltLength];
                Random random = new Random();
                random.NextBytes(salt);
                byte[] plaintext = Util.ConnectBytesPA(head, salt, content);
                Console.WriteLine("sending AES packet: " + Util.ToHexString(plaintext));
                byte[] headBlock = await AES.EncryptAsync(plaintext.Take(15).ToArray(), AesKey, SendIV);
                byte[] tailBlock = new byte[0];
                if (plaintext.Length > 15)
                {
                    plaintext = plaintext.Skip(15).ToArray();
                    tailBlock = await AES.EncryptAsync(plaintext, AesKey, SendIV);
                }
                Console.WriteLine("encrypted packet: " + Util.ToHexString(Util.ConnectBytesPA(new byte[1] { (byte)CryptographicAlgorithm.AES_256 }, headBlock, tailBlock)));
                parent.channel.SendAsync(Util.ConnectBytesPA(new byte[1] { (byte)CryptographicAlgorithm.AES_256 }, headBlock, tailBlock));
            }
            catch (System.Security.Cryptography.CryptographicException ex) //Invalid key/iv
            {
                parent.ExceptionHandler.HandleException(ex);
            }
        }
        internal abstract string PublicKey { get; }
        internal abstract string Keypair { get; }
        internal abstract byte[] AesKey { get; set; }
        internal abstract byte[] ReceiveIV { get; set; }
        internal abstract byte[] SendIV { get; set; }
        //  functions>
    }
}