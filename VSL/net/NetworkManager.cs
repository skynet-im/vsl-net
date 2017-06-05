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
        #region receive
        internal async Task OnDataReceiveAsync()
        {
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
                        await ReceivePacketAsync_AES_256();
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Received packet with unknown algorithm ({0})", algorithm.ToString()));
                }
            }
            catch (InvalidOperationException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return;
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
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
                    if (packet.PacketLength.Type == Packet.PacketLength.LengthType.Constant)
                    {
                        length = packet.PacketLength.Length;
                    }
                    else if (packet.PacketLength.Type == Packet.PacketLength.LengthType.UInt32)
                    {
                        length = BitConverter.ToUInt32(await parent.channel.ReadAsync(4), 0);
                    }
                    parent.handler.HandleInternalPacket(id, await parent.channel.ReadAsync(Convert.ToInt32(length)));
                }
                else
                {
                    throw new InvalidOperationException("Unknown packet id " + id);
                }
            }
            catch (ArgumentOutOfRangeException ex) //PacketBuffer
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (InvalidOperationException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotImplementedException ex) //PacketHandler
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotSupportedException ex) //PacketHandler
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
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
                    if (packet.PacketLength.Type == Packet.PacketLength.LengthType.Constant)
                    {
                        length = packet.PacketLength.Length;
                    }
                    else if (packet.PacketLength.Type == Packet.PacketLength.LengthType.UInt32)
                    {
                        length = BitConverter.ToUInt32(plaintext.Take(4).ToArray(), 0);
                        plaintext = plaintext.Skip(4).ToArray();
                    }
                    parent.handler.HandleInternalPacket(id, plaintext.Take(Convert.ToInt32(length)).ToArray());
                }
                else
                {
                    throw new InvalidOperationException("Unknown packet id " + id);
                }
            }
            catch (ArgumentOutOfRangeException ex) //PacketBuffer
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (InvalidOperationException ex) //PacketHandler
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotImplementedException ex) //PacketHandler
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotSupportedException ex) //PacketHandler
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
        }
        private async Task ReceivePacketAsync_AES_256()
        {
            try
            {
                byte[] ciphertext = await parent.channel.ReadAsync(16); //TimeoutException
                byte[] plaintext = await AES.DecryptAsync(ciphertext, AesKey, ReceiveIV); //CryptographicException
                byte id = plaintext.Take(1).ToArray()[0];
                plaintext = plaintext.Skip(1).ToArray();
                Packet.IPacket packet;
                bool success = parent.handler.TryGetPacket(id, out packet);
                uint length = 0;
                if (success && packet.PacketLength.Type == Packet.PacketLength.LengthType.Constant)
                {
                    length = packet.PacketLength.Length;
                }
                else
                {
                    length = BitConverter.ToUInt32(plaintext.Take(4).ToArray(), 0);
                    plaintext = plaintext.Skip(4).ToArray();
                }
                if (length > plaintext.Length - 2) // 2 random bytes in the header for more security
                {
                    int pendingLength = Convert.ToInt32(length - plaintext.Length + 2);
                    int pendingBlocks = Convert.ToInt32(Math.Ceiling((pendingLength + 1) / 16d)); // round up, first blocks only 15 bytes (padding)
                    ciphertext = await parent.channel.ReadAsync(pendingBlocks * 16);
                    plaintext = plaintext.Concat(await AES.DecryptAsync(ciphertext, AesKey, ReceiveIV)).ToArray();
                }
                int startIndex = Convert.ToInt32(plaintext.Length - length);
                byte[] content = plaintext.Skip(startIndex).ToArray(); // remove random bytes
                parent.Logger.d(string.Format("Received AES packet with native ID {0} and {1}bytes length.", id, content.Length));
                if (success)
                {
                    parent.handler.HandleInternalPacket(id, content);
                }
                else
                {
                    parent.OnPacketReceived(id, content);
                }
                ciphertext = null;
                plaintext = null;
            }
            catch (ArgumentOutOfRangeException ex) //PacketBuffer
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotImplementedException ex) //PacketHandler
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotSupportedException ex) //PacketHandler
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
        }
        #endregion receive
        #region send
        internal Task SendPacketAsync(byte id, byte[] content)
        {
            byte[] head = new byte[1] { id };
            head = head.Concat(BitConverter.GetBytes(Convert.ToUInt32(content.Length))).ToArray();
            return SendPacketAsync(CryptographicAlgorithm.AES_256, head, content);
        }
        internal Task SendPacketAsync(Packet.IPacket packet)
        {
            return SendPacketAsync(CryptographicAlgorithm.AES_256, packet);
        }
        internal Task SendPacketAsync(CryptographicAlgorithm alg, Packet.IPacket packet)
        {
            byte[] head = new byte[1] { packet.PacketID };
            PacketBuffer buf = new PacketBuffer();
            packet.WritePacket(buf);
            byte[] content = buf.ToArray();
            buf.Dispose();
            if (packet.PacketLength.Type == Packet.PacketLength.LengthType.UInt32)
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
            parent.channel.SendAsync((new byte[1] { (byte)CryptographicAlgorithm.None }).Concat(head).Concat(content).ToArray());
        }
        private async Task SendPacketAsync_RSA_2048(byte[] head, byte[] content)
        {
            try
            {
                byte[] ciphertext = await RSA.EncryptAsync(head.Concat(content).ToArray(), PublicKey);
                parent.channel.SendAsync((new byte[1] { (byte)CryptographicAlgorithm.RSA_2048 }).Concat(ciphertext).ToArray());
                head = null;
                content = null;
            }
            catch (System.Security.Cryptography.CryptographicException ex) //Invalid key
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotImplementedException ex) //Keys
            {
                parent.ExceptionHandler.CloseConnection(ex);
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
                byte[] salt = new byte[saltLength];
                Random random = new Random();
                random.NextBytes(salt);
                byte[] plaintext = head.Concat(salt).Concat(content).ToArray();
                byte[] headBlock = await AES.EncryptAsync(plaintext.Take(15).ToArray(), AesKey, SendIV);
                byte[] tailBlock = new byte[0];
                if (plaintext.Length > 15)
                {
                    plaintext = plaintext.Skip(15).ToArray();
                    tailBlock = await AES.EncryptAsync(plaintext, AesKey, SendIV);
                }
                parent.channel.SendAsync((new byte[1] { (byte)CryptographicAlgorithm.AES_256 }).Concat(headBlock).Concat(tailBlock).ToArray());
                parent.Logger.d(string.Format("Sent AES packet with native ID {0} and {1}bytes length", head[0], content.Length));
                head = null;
                content = null;
                plaintext = null;
                headBlock = null;
                tailBlock = null;
            }
            catch (System.Security.Cryptography.CryptographicException ex) //Invalid key/iv
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
        }
        #endregion send
        internal abstract string PublicKey { get; }
        internal abstract string Keypair { get; }
        internal abstract byte[] AesKey { get; set; }
        internal abstract byte[] ReceiveIV { get; set; }
        internal abstract byte[] SendIV { get; set; }
        //  functions>
    }
}