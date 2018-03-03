using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using VSL.Crypt;
using VSL.Net;

namespace VSL
{
    /// <summary>
    /// Responsible for cryptography management
    /// </summary>
    internal sealed class NetworkManager : IDisposable
    {
        // <fields
        internal VSLSocket parent;
        internal bool Ready4Aes = false;
        private string rsaKey;
        private HMACSHA256 hmacProvider;
        //  fields>
        // <constructor
        internal NetworkManager(VSLSocket parent, string rsaKey)
        {
            this.parent = parent;
            this.rsaKey = rsaKey;
        }
        //  constructor>
        // <functions
        #region receive
        internal bool OnDataReceive()
        {
            try
            {
                if (!parent.channel.TryRead(out byte[] buf, 1))
                    return false;
                CryptoAlgorithm algorithm = (CryptoAlgorithm)buf[0];
                switch (algorithm)
                {
                    case CryptoAlgorithm.None:
                        return ReceivePacket_Plaintext();
                    case CryptoAlgorithm.RSA_2048_OAEP:
                        return ReceivePacket_RSA_2048_OAEP();
                    case CryptoAlgorithm.AES_256_CBC_SP:
                        if (!Ready4Aes)
                        {
                            parent.ExceptionHandler.CloseConnection("InvalidOperation",
                                "Not ready to receive an AES packet, because key exchange is not finished yet.\r\n" +
                                "\tat NetworkManager.OnDataReceive()");
                            return false;
                        }
                        if (parent.ConnectionVersion.HasValue && parent.ConnectionVersion.Value >= 2)
                        {
                            parent.ExceptionHandler.CloseConnection("InvalidAlgorithm",
                                "VSL versions 1.2 and later are not allowed to use an old, insecure algorithm.\r\n" +
                                "\tat NetworkManager.OnDataReceive()");
                            return false;
                        }
                        return ReceivePacket_AES_256_CBC_SP();

                    case CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3:
                        if (!Ready4Aes)
                        {
                            parent.ExceptionHandler.CloseConnection("InvalidOperation",
                                "Not ready to receive an AES packet, because key exchange is not finished yet.\r\n" +
                                "\tat NetworkManager.OnDataReceive()");
                            return false;
                        }
                        if (parent.ConnectionVersion.HasValue && parent.ConnectionVersion.Value < 2)
                        {
                            parent.ExceptionHandler.CloseConnection("InvalidAlgorithm",
                                "VSL versions older than 1.2 should not be able to use CryptographicAlgorithm.AES_256_CBC_MP2.\r\n" +
                                "\tat NetworkManager.OnDataReceive()");
                            return false;
                        }
                        return ReceivePacket_AES_256_CBC_HMAC_SHA_256_MP3();

                    default:
                        parent.ExceptionHandler.CloseConnection("InvalidAlgorithm",
                            $"Received packet with unknown algorithm ({algorithm}).\r\n" +
                            $"\tat NetworkManager.OnDataReceive()");
                        return false;
                }
            }
            catch (OperationCanceledException) // NetworkChannel.Read()
            {
                return false; // Already shutting down...
            }
            catch (TimeoutException ex) // NetworkChannel.Read()
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
            catch (Exception ex)
            {
                parent.ExceptionHandler.CloseUncaught(ex);
                return false;
            }
        }
        private bool ReceivePacket_Plaintext()
        {
            try
            {
                byte id; // read packet id
                {
                    if (!parent.channel.TryRead(out byte[] buf, 1))
                        return false;
                    id = buf[0];
                }

                bool success = parent.handler.TryGetPacket(id, out Packet.IPacket packet);
                if (!success)
                {
                    parent.ExceptionHandler.CloseConnection("UnknownPacket",
                        $"Received unknown internal plaintext packet with id {id}\r\n" +
                        "\tat NetworkManager.ReceivePacket_Plaintext()");
                    return false;
                }

                uint length = 0; // read packet length
                if (packet.ConstantLength.HasValue)
                    length = packet.ConstantLength.Value;
                else
                {
                    if (!parent.channel.TryRead(out byte[] buf, 4))
                        return false;
                    length = BitConverter.ToUInt32(buf, 0);
                    if (length > Constants.MaxPacketSize)
                    {
                        parent.ExceptionHandler.CloseConnection("TooBigPacket",
                            $"Tried to receive a packet of {length} bytes. Maximum admissible are {Constants.MaxPacketSize} bytes.\r\n" +
                            "\tat NetworkManager.ReceivePacket_Plaintext()");
                        return false;
                    }
                }

                {
                    if (!parent.channel.TryRead(out byte[] buf, Convert.ToInt32(length))) // read packet content
                        return false;
                    return parent.handler.HandleInternalPacket(id, buf, CryptoAlgorithm.None);
                }
            }
            catch (ArgumentOutOfRangeException ex) // PacketHandler.HandleInternalPacket() => IPacket.ReadPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            return false;
        }
        private bool ReceivePacket_RSA_2048_OAEP()
        {
            try
            {
                int index = 1;
                if (!parent.channel.TryRead(out byte[] ciphertext, 256))
                    return false;
                byte[] plaintext = Crypt.RsaStatic.DecryptBlock(ciphertext, rsaKey);
                byte id = plaintext[0]; // index = 1
                bool success = parent.handler.TryGetPacket(id, out Packet.IPacket packet);
                if (success)
                {
                    uint length = 0;
                    if (packet.ConstantLength.HasValue)
                        length = packet.ConstantLength.Value;
                    else
                    {
                        length = BitConverter.ToUInt32(Util.TakeBytes(plaintext, 4, index), 0);
                        if (length > 209) // 214 - 1 (id) - 4 (uint) => 209
                        {
                            parent.ExceptionHandler.CloseConnection("TooBigPacket",
                                $"Tried to receive a packet of {length} bytes. Maximum admissible are 209 bytes.\r\n" +
                                "\tat NetworkManager.ReceivePacket_RSA_2048()");
                            return false;
                        }
                        index += 4;
                    }
                    return parent.handler.HandleInternalPacket(id, Util.TakeBytes(plaintext, Convert.ToInt32(length), index), CryptoAlgorithm.RSA_2048_OAEP);
                }
                else
                {
                    parent.ExceptionHandler.CloseConnection("UnknownPacket",
                        $"Received unknown internal RSA packet with id {id}\r\n" +
                        "\tat NetworkManager.ReceivePacket_RSA_2048()");
                    return false;
                }
            }
            catch (ArgumentOutOfRangeException ex) // PacketHandler.HandleInternalPacket() => IPacket.ReadPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (CryptographicException ex) // RSA.DecryptBlock()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            return false;
        }
        private bool ReceivePacket_AES_256_CBC_SP()
        {
            try
            {
                int index = 1;
                if (!parent.channel.TryRead(out byte[] ciphertext, 16))
                    return false;
                byte[] plaintext = AesStatic.Decrypt(ciphertext, AesKey, ReceiveIV); //CryptographicException
                byte id = plaintext[0];
                bool success = parent.handler.TryGetPacket(id, out Packet.IPacket packet);
                uint length = 0;
                if (success && packet.ConstantLength.HasValue)
                    length = packet.ConstantLength.Value;
                else
                {
                    length = BitConverter.ToUInt32(Util.TakeBytes(plaintext, 4, index), 0);
                    index += 4;
                    if (length > Constants.MaxPacketSize)
                    {
                        parent.ExceptionHandler.CloseConnection("TooBigPacket",
                            $"Tried to receive a packet of {length} bytes. Maximum admissible are {Constants.MaxPacketSize} bytes.\r\n" +
                            "\tat NetworkManager.ReceivePacket_Plaintext()");
                        return false;
                    }
                }
                plaintext = Util.SkipBytes(plaintext, index);
                if (length > plaintext.Length - 2) // 2 random bytes
                {
                    int pendingLength = Convert.ToInt32(length - plaintext.Length + 2);
                    int pendingBlocks = Convert.ToInt32(Math.Ceiling((pendingLength + 1) / 16d)); // round up, first blocks only 15 bytes (padding)
                    if (!parent.channel.TryRead(out ciphertext, pendingBlocks * 16))
                        return false;
                    plaintext = Util.ConnectBytes(plaintext, AesStatic.Decrypt(ciphertext, AesKey, ReceiveIV));
                }
                int startIndex = Convert.ToInt32(plaintext.Length - length);
                byte[] content = Util.SkipBytes(plaintext, startIndex); // remove random bytes
                if (success)
                {
                    if (parent.Logger.InitD)
                        parent.Logger.D($"Received internal insecure AES packet: ID={id} Length={content.Length}");
                    return parent.handler.HandleInternalPacket(id, content, CryptoAlgorithm.AES_256_CBC_SP);
                }
                else
                {
                    if (parent.Logger.InitD)
                        parent.Logger.D($"Received external insecure AES packet: ID={255 - id} Length={content.Length}");
                    parent.OnPacketReceived(id, content);
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException ex) // PacketHandler.HandleInternalPacket() => IPacket.ReadPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (CryptographicException ex) // AES.Decrypt()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            return false;
        }
        private bool ReceivePacket_AES_256_CBC_HMAC_SHA_256_MP3()
        {
            try
            {
                if (!parent.channel.TryRead(out byte[] buf, 35)) // 3 (length) + 32 (HMAC)
                    return false;
                int blocks = Convert.ToInt32(UInt24.FromBytes(buf, 0));
                byte[] hmac = Util.TakeBytes(buf, 32, 3);
                if (!parent.channel.TryRead(out byte[] cipherblock, (blocks + 2) * 16)) // inclusive iv
                    return false;
                if (!hmac.SequenceEqual(hmacProvider.ComputeHash(cipherblock)))
                {
                    parent.ExceptionHandler.CloseConnection("MessageCorrupted",
                        "The integrity checking resulted in a corrupted message.\r\n" +
                        "\tat NetworkManager.ReceivePacket_AES_256_CBC_HMAC_SHA_256_MP3()\r\n" +
                        "\tblock count: " + blocks);
                    return false;
                }
                byte[] iv = Util.TakeBytes(cipherblock, 16);
                byte[] ciphertext = Util.SkipBytes(cipherblock, 16);
                using (PacketBuffer plaintext = new PacketBuffer(AesStatic.Decrypt(ciphertext, AesKey, iv)))
                {
                    while (plaintext.Position < plaintext.Length - 1)
                    {
                        byte id = plaintext.ReadByte();
                        bool success = parent.handler.TryGetPacket(id, out Packet.IPacket packet);
                        uint length = 0;
                        if (success && packet.ConstantLength.HasValue)
                            length = packet.ConstantLength.Value;
                        else
                            length = plaintext.ReadUInt();
                        if (length > plaintext.Pending)
                        {
                            parent.ExceptionHandler.CloseConnection("TooBigPacket",
                                $"Tried to receive a packet with {length} bytes length although only {plaintext.Pending} bytes are available.\r\n" +
                                $"\tat NetworkManager.ReceivePacket_AES_256_CBC_MP2()");
                            return false;
                        }
                        byte[] content = plaintext.ReadByteArray((int)length);
                        if (success)
                        {
                            if (parent.Logger.InitD)
                                parent.Logger.D($"Received internal AES packet: ID={id} Length={content.Length}");
                            if (!parent.handler.HandleInternalPacket(id, content, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3))
                                return false;
                        }
                        else
                        {
                            if (parent.Logger.InitD)
                                parent.Logger.D($"Received external AES packet: ID={255 - id} Length={content.Length}");
                            parent.OnPacketReceived(id, content);
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex) // PacketHandler.HandleInternalPacket() => IPacket.ReadPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (CryptographicException ex) // AES.Decrypt()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            return true;
        }
        #endregion receive
        #region send
        internal bool SendPacket(byte id, byte[] content) => SendPacket(VersionManager.GetNetworkAlgorithm(parent.ConnectionVersion), id, content);
        internal bool SendPacket(CryptoAlgorithm alg, byte id, byte[] content) => SendPacket(alg, id, true, content);
        internal bool SendPacket(Packet.IPacket packet) => SendPacket(VersionManager.GetNetworkAlgorithm(parent.ConnectionVersion), packet);
        internal bool SendPacket(CryptoAlgorithm alg, Packet.IPacket packet)
        {
            byte[] content;
            using (PacketBuffer buf = new PacketBuffer())
            {
                packet.WritePacket(buf);
                content = buf.ToArray();
            }
            return SendPacket(alg, packet.PacketId, !packet.ConstantLength.HasValue, content);
        }
        private bool SendPacket(CryptoAlgorithm alg, byte realId, bool size, byte[] content)
        {
            switch (alg)
            {
                case CryptoAlgorithm.None:
                    return SendPacket_Plaintext(realId, size, content);
                case CryptoAlgorithm.RSA_2048_OAEP:
                    return SendPacket_RSA_2048_OAEP(realId, size, content);
                case CryptoAlgorithm.AES_256_CBC_SP:
                    return SendPacket_AES_256_CBC_SP(realId, size, content);
                case CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3:
                    return SendPacket_AES_256_CBC_HMAC_SHA256_MP3(realId, size, content);
                default:
                    throw new InvalidOperationException();
            }
        }
        private bool SendPacket_Plaintext(byte realId, bool size, byte[] content)
        {
            int length = 2 + (size ? 4 : 0) + content.Length;
            byte[] buf;
            using (PacketBuffer pbuf = new PacketBuffer(length))
            {
                pbuf.WriteByte((byte)CryptoAlgorithm.None);
                pbuf.WriteByte(realId);
                if (size) pbuf.WriteUInt((uint)content.Length);
                pbuf.WriteByteArray(content, false);
                buf = pbuf.ToArray();
            }
            return buf.Length == parent.channel.Send(buf);
        }
        private bool SendPacket_RSA_2048_OAEP(byte realId, bool size, byte[] content)
        {
            try
            {
                int length = 1 + (size ? 4 : 0) + content.Length;
                byte[] plain;
                using (PacketBuffer pbuf = new PacketBuffer(length))
                {
                    pbuf.WriteByte(realId);
                    if (size) pbuf.WriteUInt((uint)content.Length);
                    pbuf.WriteByteArray(content, false);
                    plain = pbuf.ToArray();
                }
                byte[] ciphertext = RsaStatic.EncryptBlock(plain, rsaKey);
                byte[] buf = new byte[1 + ciphertext.Length];
                buf[0] = (byte)CryptoAlgorithm.RSA_2048_OAEP;
                Array.Copy(ciphertext, 0, buf, 1, ciphertext.Length);
                return buf.Length == parent.channel.Send(buf);
            }
            catch (CryptographicException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
            catch (NotImplementedException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
        }
        private bool SendPacket_AES_256_CBC_SP(byte realId, bool size, byte[] content)
        {
            try
            {
                int headLength = 1 + (size ? 4 : 0);
                int blocks = 1;
                int saltLength;
                if (headLength + 2 + content.Length < 16) // 2 random bytes
                    saltLength = 15 - headLength - content.Length; // 16 - 1 (padding) = 15
                else
                {
                    blocks = Util.GetTotalSize(headLength + 4 + content.Length, 16) / 16; // 2 random bytes + 2x padding
                    saltLength = blocks * 16 - headLength - content.Length - 2; // 2x padding
                }
                byte[] salt = new byte[saltLength];
                Random random = new Random();
                random.NextBytes(salt);
                byte[] plaintext;
                using (PacketBuffer pbuf = new PacketBuffer(headLength + saltLength + content.Length))
                {
                    pbuf.WriteByte(realId);
                    if (size) pbuf.WriteUInt((uint)content.Length);
                    pbuf.WriteByteArray(salt, false);
                    pbuf.WriteByteArray(content, false);
                    plaintext = pbuf.ToArray();
                }
                byte[] headBlock = AesStatic.Encrypt(Util.TakeBytes(plaintext, 15), AesKey, SendIV);
                byte[] tailBlock = new byte[0];
                if (plaintext.Length > 15)
                {
                    plaintext = Util.SkipBytes(plaintext, 15);
                    tailBlock = AesStatic.Encrypt(plaintext, AesKey, SendIV);
                }
                byte[] buf;
                using (PacketBuffer pbuf = new PacketBuffer(headBlock.Length + tailBlock.Length))
                {
                    pbuf.WriteByte((byte)CryptoAlgorithm.AES_256_CBC_SP);
                    pbuf.WriteByteArray(headBlock, false);
                    pbuf.WriteByteArray(tailBlock, false);
                    buf = pbuf.ToArray();
                }
                bool success = buf.Length == parent.channel.Send(buf);
                if (realId <= 9 && parent.Logger.InitD)
                    parent.Logger.D(string.Format("Sent internal AES packet: ID={0} Length={1} {2}b", realId, buf.Length, blocks));
                else if (parent.Logger.InitD)
                    parent.Logger.D(string.Format("Sent external AES packet: ID={0} Length={1} {2}b", 255 - realId, buf.Length, blocks));
                return success;
            }
            catch (CryptographicException ex) //Invalid key/iv
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
        }
        private bool SendPacket_AES_256_CBC_HMAC_SHA256_MP3(byte realId, bool size, byte[] content)
        {
            int headLength = 1 + (size ? 4 : 0) + content.Length;
            byte[] plaintext;
            using (PacketBuffer pbuf = new PacketBuffer(headLength))
            {
                pbuf.WriteByte(realId);
                if (size) pbuf.WriteUInt((uint)content.Length);
                pbuf.WriteByteArray(content, false);
                plaintext = pbuf.ToArray();
            }
            byte[] iv = AesStatic.GenerateIV();
            byte[] ciphertext = AesStatic.Encrypt(plaintext, AesKey, iv);
            byte[] blocks = UInt24.ToBytes((uint)ciphertext.Length / 16 - 1);
            byte[] cipherblock = Util.ConnectBytes(iv, ciphertext);
            byte[] hmac = hmacProvider.ComputeHash(cipherblock);
            byte[] buf;
            using (PacketBuffer pbuf = new PacketBuffer(1 + 3 + hmac.Length + cipherblock.Length))
            {
                pbuf.WriteByte((byte)CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3);
                pbuf.WriteByteArray(blocks, false);
                pbuf.WriteByteArray(hmac, false);
                pbuf.WriteByteArray(cipherblock, false);
                buf = pbuf.ToArray();
            }
            return buf.Length == parent.channel.Send(buf);
        }
        #endregion send
        #region keys
        /// <summary>
        /// Generates all keys and ivs and sets them.
        /// </summary>
        internal void GenerateKeys()
        {
            AesKey = AesStatic.GenerateKey();
            ReceiveIV = AesStatic.GenerateIV();
            SendIV = AesStatic.GenerateIV();
            HmacKey = Util.ConnectBytes(SendIV, ReceiveIV);
            Ready4Aes = true;
        }
        internal byte[] AesKey { get; set; }
        private byte[] _hmacKey;
        internal byte[] HmacKey
        {
            get => _hmacKey;
            set
            {
                _hmacKey = value;
                if (hmacProvider == null)
                    hmacProvider = new HMACSHA256(value);
                else
                    hmacProvider.Key = value;
            }
        }
        internal byte[] ReceiveIV { get; set; }
        internal byte[] SendIV { get; set; }
        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // -TODO: dispose managed state (managed objects).
                    hmacProvider?.Dispose();
                }

                // -TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // -TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // -TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NetworkManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // -TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        //  functions>
    }
}