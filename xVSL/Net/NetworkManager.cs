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
#if !WINDOWS_UWP
        private AesCsp enc;
        private AesCsp dec;
#endif
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
                            parent.ExceptionHandler.CloseConnection("InvalidOperation", "Not ready to receive an AES packet, because key exchange is not finished yet.\r\n\tat NetworkManager.OnDataReceive()");
                            return false;
                        }
                        if (parent.ConnectionVersion.HasValue && parent.ConnectionVersion.Value >= 2)
                        {
                            parent.ExceptionHandler.CloseConnection("InvalidAlgorithm", "VSL versions 1.2 and later are not allowed to use an old, insecure algorithm.\r\n\tat NetworkManager.OnDataReceive()");
                            return false;
                        }
                        return ReceivePacket_AES_256_CBC_SP();

                    case CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3:
                        if (!Ready4Aes)
                        {
                            parent.ExceptionHandler.CloseConnection("InvalidOperation", "Not ready to receive an AES packet, because key exchange is not finished yet.\r\n\tat NetworkManager.OnDataReceive()");
                            return false;
                        }
                        if (parent.ConnectionVersion.HasValue && parent.ConnectionVersion.Value < 2)
                        {
                            parent.ExceptionHandler.CloseConnection("InvalidAlgorithm", "VSL versions older than 1.2 should not be able to use CryptographicAlgorithm.AES_256_CBC_MP2.\r\n\tat NetworkManager.OnDataReceive()");
                            return false;
                        }
                        return ReceivePacket_AES_256_CBC_HMAC_SHA_256_MP3();

                    default:
                        parent.ExceptionHandler.CloseConnection("InvalidAlgorithm", $"Received packet with unknown algorithm ({algorithm}).\r\n\tat NetworkManager.OnDataReceive()");
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
                    parent.ExceptionHandler.CloseConnection("UnknownPacket", "Received unknown internal plaintext packet with id " + id + "\r\n\tat NetworkManager.ReceivePacket_Plaintext()");
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
                        parent.ExceptionHandler.CloseConnection("TooBigPacket", string.Format("Tried to receive a packet of {0} bytes. Maximum admissible are {1} bytes.\r\n\tat NetworkManager.ReceivePacket_Plaintext()", length, Constants.MaxPacketSize));
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
                            parent.ExceptionHandler.CloseConnection("TooBigPacket", string.Format("Tried to receive a packet of {0} bytes. Maximum admissible are 209 bytes.\r\n\tat NetworkManager.ReceivePacket_RSA_2048()", length, Constants.MaxPacketSize));
                            return false;
                        }
                        index += 4;
                    }
                    return parent.handler.HandleInternalPacket(id, Util.TakeBytes(plaintext, Convert.ToInt32(length), index), CryptoAlgorithm.RSA_2048_OAEP);
                }
                else
                {
                    parent.ExceptionHandler.CloseConnection("UnknownPacket", "Received unknown internal RSA packet with id " + id + "\r\n\tat NetworkManager.ReceivePacket_RSA_2048()");
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
#if WINDOWS_UWP
                byte[] plaintext = AesStatic.Decrypt(ciphertext, _aesKey, _receiveIV);
#else
                byte[] plaintext = dec.Decrypt(ciphertext); //CryptographicException
#endif
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
                        parent.ExceptionHandler.CloseConnection("TooBigPacket", string.Format("Tried to receive a packet of {0} bytes. Maximum admissible are {1} bytes.\r\n\tat NetworkManager.ReceivePacket_Plaintext()", length, Constants.MaxPacketSize));
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
#if WINDOWS_UWP
                    plaintext = Util.ConnectBytes(plaintext, AesStatic.Decrypt(ciphertext, _aesKey, _receiveIV));
#else
                    plaintext = Util.ConnectBytes(plaintext, dec.Decrypt(ciphertext));
#endif
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
                        "The integrity checking resulted in a corrupted message.\r\n\t" +
                        "at NetworkManager.ReceivePacket_AES_256_CBC_HMAC_SHA_256_MP3()\r\n\t" +
                        "block count: " + blocks);
                    return false;
                }
                byte[] iv = Util.TakeBytes(cipherblock, 16);
                byte[] ciphertext = Util.SkipBytes(cipherblock, 16);
#if WINDOWS_UWP
                byte[] b_plaintext = AesStatic.Decrypt(ciphertext, _aesKey, iv);
#else
                dec.IV = iv;
                byte[] b_plaintext = dec.Decrypt(ciphertext);
#endif
                using (System.IO.MemoryStream ms_plaintext = new System.IO.MemoryStream(b_plaintext))
                {
                    using (System.IO.BinaryReader plaintext = new System.IO.BinaryReader(ms_plaintext))
                    {
                        while (ms_plaintext.Position < ms_plaintext.Length - 1)
                        {
                            byte id = plaintext.ReadByte();
                            bool success = parent.handler.TryGetPacket(id, out Packet.IPacket packet);
                            uint length = 0;
                            if (success && packet.ConstantLength.HasValue)
                                length = packet.ConstantLength.Value;
                            else
                                length = plaintext.ReadUInt32();
                            long pending = ms_plaintext.Length - ms_plaintext.Position;
                            if (length > pending)
                            {
                                parent.ExceptionHandler.CloseConnection("TooBigPacket", $"Tried to receive a packet with {length} bytes length although only {pending} bytes are available.\r\n\tat NetworkManager.ReceivePacket_AES_256_CBC_MP2()");
                                return false;
                            }
                            byte[] content = plaintext.ReadBytes(Convert.ToInt32(length));
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
                                    parent.Logger.D($"Received external insecure AES packet: ID={255 - id} Length={content.Length}");
                                parent.OnPacketReceived(id, content);
                            }
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
        internal bool SendPacket(byte id, byte[] content)
        {
            byte[] head = Util.ConnectBytes(new byte[1] { id }, BitConverter.GetBytes(Convert.ToUInt32(content.Length)));
            if (parent.ConnectionVersion < 2)
                return SendPacket(CryptoAlgorithm.AES_256_CBC_SP, head, content);
            if (parent.ConnectionVersion == 2)
                return SendPacket(CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3, head, content);
            return false;
        }
        internal bool SendPacket(Packet.IPacket packet)
        {
            if (parent.ConnectionVersion < 2)
                return SendPacket(CryptoAlgorithm.AES_256_CBC_SP, packet);
            if (parent.ConnectionVersion == 2)
                return SendPacket(CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3, packet);
            return false;
        }
        internal bool SendPacket(CryptoAlgorithm alg, Packet.IPacket packet)
        {
            byte[] head = new byte[1] { packet.PacketID };
            PacketBuffer buf = new PacketBuffer();
            packet.WritePacket(buf);
            byte[] content = buf.ToArray();
            buf.Dispose();
            if (!packet.ConstantLength.HasValue)
                head = Util.ConnectBytes(head, BitConverter.GetBytes(Convert.ToUInt32(content.Length)));
            return SendPacket(alg, head, content);
        }
        internal bool SendPacket(CryptoAlgorithm alg, byte[] head, byte[] content)
        {
            switch (alg)
            {
                case CryptoAlgorithm.None:
                    return SendPacket_Plaintext(head, content);
                case CryptoAlgorithm.RSA_2048_OAEP:
                    return SendPacket_RSA_2048_OAEP(head, content);
                case CryptoAlgorithm.AES_256_CBC_SP:
                    return SendPacket_AES_256_CBC_SP(head, content);
                case CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3:
                    return SendPacket_AES_256_CBC_HMAC_SH256_MP3(head, content);
                default:
                    throw new InvalidOperationException();
            }
        }
        private bool SendPacket_Plaintext(byte[] head, byte[] content)
        {
            byte[] buf = Util.ConnectBytes(GetPrefix(CryptoAlgorithm.None), head, content);
            return buf.Length == parent.channel.Send(buf);
        }
        private bool SendPacket_RSA_2048_OAEP(byte[] head, byte[] content)
        {
            try
            {
                byte[] ciphertext = Crypt.RsaStatic.EncryptBlock(Util.ConnectBytes(head, content), rsaKey);
                byte[] buf = Util.ConnectBytes(GetPrefix(CryptoAlgorithm.RSA_2048_OAEP), ciphertext);
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
        private bool SendPacket_AES_256_CBC_SP(byte[] head, byte[] content)
        {
            try
            {
                int blocks = 1;
                int saltLength;
                if (head.Length + 2 + content.Length < 16) // 2 random bytes
                {
                    saltLength = 15 - head.Length - content.Length; // padding
                }
                else
                {
                    blocks = Convert.ToInt32(Math.Ceiling((head.Length + 4 + content.Length) / 16d)); // 2 random bytes + 2x padding
                    saltLength = blocks * 16 - head.Length - content.Length - 2; // padding
                }
                byte[] salt = new byte[saltLength];
                Random random = new Random();
                random.NextBytes(salt);
                byte[] plaintext = Util.ConnectBytes(head, salt, content);
#if WINDOWS_UWP
                byte[] headBlock = AesStatic.Encrypt(Util.TakeBytes(plaintext, 15), _aesKey, _sendIV);
#else
                byte[] headBlock = enc.Encrypt(Util.TakeBytes(plaintext, 15));
#endif
                byte[] tailBlock = new byte[0];
                if (plaintext.Length > 15)
                {
                    plaintext = Util.SkipBytes(plaintext, 15);
#if WINDOWS_UWP
                    tailBlock = AesStatic.Encrypt(plaintext, _aesKey, _sendIV);
#else
                    tailBlock = enc.Encrypt(plaintext);
#endif
                }
                byte[] buf = Util.ConnectBytes(GetPrefix(CryptoAlgorithm.AES_256_CBC_SP), headBlock, tailBlock);
                bool success = buf.Length == parent.channel.Send(buf);
                if (head[0] <= 9 && parent.Logger.InitD)
                    parent.Logger.D(string.Format("Sent internal AES packet: ID={0} Length={1} {2}b", head[0], buf.Length, blocks));
                else if (parent.Logger.InitD)
                    parent.Logger.D(string.Format("Sent external AES packet: ID={0} Length={1} {2}b", 255 - head[0], buf.Length, blocks));
                return success;
            }
            catch (System.Security.Cryptography.CryptographicException ex) //Invalid key/iv
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
        }
        private bool SendPacket_AES_256_CBC_HMAC_SH256_MP3(byte[] head, byte[] content)
        {
#if WINDOWS_UWP
            byte[] iv = AesStatic.GenerateIV();
            byte[] ciphertext = AesStatic.Encrypt(Util.ConnectBytes(head, content), _aesKey, iv);
#else
            byte[] iv = enc.GenerateIV(true);
            byte[] ciphertext = enc.Encrypt(Util.ConnectBytes(head, content));
#endif
            byte[] blocks = UInt24.ToBytes(Convert.ToUInt32(ciphertext.Length / 16 - 1));
            byte[] cipherblock = Util.ConnectBytes(iv, ciphertext);
            byte[] hmac = hmacProvider.ComputeHash(cipherblock);
            byte[] buf = Util.ConnectBytes(GetPrefix(CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3), blocks, hmac, cipherblock);
            return buf.Length == parent.channel.Send(buf);
        }
        #endregion send
        #region keys
        /// <summary>
        /// Generates all keys and ivs and sets them.
        /// </summary>
        internal void GenerateKeys()
        {
#if WINDOWS_UWP
            _aesKey = AesStatic.GenerateKey();
            _receiveIV = AesStatic.GenerateIV();
            _sendIV = AesStatic.GenerateIV();
#else
            enc = new AesCsp();
            _aesKey = enc.GenerateKey(true);
            _receiveIV = enc.GenerateIV(false);
            _sendIV = enc.GenerateIV(true);
            dec = new AesCsp(_aesKey, _receiveIV);
            HmacKey = Util.ConnectBytes(_sendIV, _receiveIV);
#endif
            Ready4Aes = true;
        }
        private byte[] _aesKey;
        internal byte[] AesKey
        {
            get
            {
                return _aesKey;
            }
            set
            {
                _aesKey = value;
#if !WINDOWS_UWP
                if (enc == null)
                    enc = new AesCsp(value);
                else
                    enc.Key = value;
                if (dec == null)
                    dec = new AesCsp(value);
                else
                    dec.Key = value;
#endif
            }
        }
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
        private byte[] _receiveIV;
        internal byte[] ReceiveIV
        {
            get
            {
                return _receiveIV;
            }
            set
            {
#if WINDOWS_UWP
                _receiveIV = value;
#else
                if (dec == null) throw new InvalidOperationException("You have to asign the key before the iv");
                _receiveIV = value;
                dec.IV = value;
#endif
            }
        }
        private byte[] _sendIV;
        internal byte[] SendIV
        {
            get
            {
                return _sendIV;
            }
            set
            {
#if WINDOWS_UWP
                _sendIV = value;
#else
                if (enc == null) throw new InvalidOperationException("You have to asign the key before the iv");
                _sendIV = value;
                enc.IV = value;
#endif
            }
        }
        #endregion
        private byte[] GetPrefix(CryptoAlgorithm algorithm)
        {
            return new byte[1] { (byte)algorithm };
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // -TODO: dispose managed state (managed objects).
#if !WINDOWS_UWP
                    enc?.Dispose();
                    dec?.Dispose();
#endif
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