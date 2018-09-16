using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using VSL.BinaryTools;
using VSL.Crypt;

namespace VSL.Network
{
    /// <summary>
    /// Responsible for cryptography management
    /// </summary>
    internal sealed class NetworkManager : IDisposable
    {
        // <fields
        private VSLSocket parent;
        internal bool Ready4Aes = false;
        private readonly RSAParameters rsaKey;
        private HMACSHA256 hmacProvider;
        //  fields>
        // <constructor
        internal NetworkManager(VSLSocket parent, RSAParameters rsaKey)
        {
            this.parent = parent;
            this.rsaKey = rsaKey;
        }
        //  constructor>
        // <functions
        #region receive
        internal async Task<bool> ReceivePacketAsync()
        {
            try
            {
                byte[] buffer = new byte[1];
                if (!await parent.Channel.ReceiveAsync(buffer, 0, 1))
                    return false;
                CryptoAlgorithm algorithm = (CryptoAlgorithm)buffer[0];
                switch (algorithm)
                {
                    case CryptoAlgorithm.None:
                        return await ReceivePacketAsync_Plaintext();
                    case CryptoAlgorithm.RSA_2048_OAEP:
                        return await ReceivePacketAsync_RSA_2048_OAEP();
                    case CryptoAlgorithm.AES_256_CBC_SP:
                        if (!Ready4Aes)
                        {
                            parent.ExceptionHandler.CloseConnection("InvalidOperation",
                                "Not ready to receive an AES packet, because key exchange is not finished yet.",
                                nameof(NetworkManager), nameof(ReceivePacketAsync));
                            return false;
                        }
                        if (parent.ConnectionVersion.HasValue && parent.ConnectionVersion.Value >= 2)
                        {
                            parent.ExceptionHandler.CloseConnection("InvalidAlgorithm",
                                "VSL versions 1.2 and later are not allowed to use an old, insecure algorithm.",
                                nameof(NetworkManager), nameof(ReceivePacketAsync));
                            return false;
                        }
                        return await ReceivePacketAsync_AES_256_CBC_SP();

                    case CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3:
                        if (!Ready4Aes)
                        {
                            parent.ExceptionHandler.CloseConnection("InvalidOperation",
                                "Not ready to receive an AES packet, because key exchange is not finished yet.",
                                nameof(NetworkManager), nameof(ReceivePacketAsync));
                            return false;
                        }
                        if (parent.ConnectionVersion.HasValue && parent.ConnectionVersion.Value < 2)
                        {
                            parent.ExceptionHandler.CloseConnection("InvalidAlgorithm",
                                "VSL versions older than 1.2 should not be able to use CryptographicAlgorithm.AES_256_CBC_MP2.",
                                nameof(NetworkManager), nameof(ReceivePacketAsync));
                            return false;
                        }
                        return await ReceivePacketAsync_AES_256_CBC_HMAC_SHA_256_MP3();

                    default:
                        parent.ExceptionHandler.CloseConnection("InvalidAlgorithm",
                            $"Received packet with unknown algorithm ({algorithm}).",
                            nameof(NetworkManager), nameof(ReceivePacketAsync));
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
#if DEBUG
            catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
#else
            catch (Exception ex)
#endif
            {
                parent.ExceptionHandler.CloseUncaught(ex);
                return false;
            }
        }
        private async Task<bool> ReceivePacketAsync_Plaintext()
        {
            CryptoAlgorithm alg = CryptoAlgorithm.None;
            byte id; // read packet id
            byte[] buffer = new byte[1];
            if (!await parent.Channel.ReceiveAsync(buffer, 0, 1))
                return false;
            id = buffer[0];

            if (!AssertInternal(id, alg, nameof(ReceivePacketAsync_Plaintext)) ||
                !parent.Handler.ValidatePacket(id, alg, out PacketRule rule))
                return false;

            uint length; // read packet length
            if (rule.Packet.ConstantLength.HasValue)
                length = rule.Packet.ConstantLength.Value;
            else
            {
                buffer = new byte[4];
                if (!await parent.Channel.ReceiveAsync(buffer, 0, 4))
                    return false;
                length = BitConverter.ToUInt32(buffer, 0);
                if (!AssertSize(parent.Settings.MaxPacketSize, (int)length, nameof(ReceivePacketAsync_Plaintext)))
                    return false;
            }

            buffer = new byte[length];
            if (!await parent.Channel.ReceiveAsync(buffer, 0, (int)length)) // read packet content
                return false;
#if DEBUG
            parent.Log($"Received internal plaintext packet: ID={id} Length={length}");
#endif
            return await parent.Handler.HandleInternalPacketAsync(rule, buffer);
        }
        private async Task<bool> ReceivePacketAsync_RSA_2048_OAEP()
        {
            CryptoAlgorithm alg = CryptoAlgorithm.RSA_2048_OAEP;
            try
            {
                int index = 1;
                byte[] ciphertext = new byte[256];
                if (!await parent.Channel.ReceiveAsync(ciphertext, 0, 256))
                    return false;
                byte[] plaintext = RsaStatic.DecryptBlock(ciphertext, rsaKey);
                byte id = plaintext[0]; // index = 1
                if (!AssertInternal(id, alg, nameof(ReceivePacketAsync_RSA_2048_OAEP)) ||
                    !parent.Handler.ValidatePacket(id, alg, out PacketRule rule))
                    return false;

                uint length = 0;
                if (rule.Packet.ConstantLength.HasValue)
                    length = rule.Packet.ConstantLength.Value;
                else
                {
                    length = BitConverter.ToUInt32(plaintext, index);
                    index += 4;
                    if (!AssertSize(209, (int)length, nameof(ReceivePacketAsync_RSA_2048_OAEP)))
                        return false; // 214 - 1 (id) - 4 (uint) => 209
                }
#if DEBUG
                parent.Log($"Received internal RSA packet: ID={id} Length={length}");
#endif
                return await parent.Handler.HandleInternalPacketAsync(rule, plaintext.TakeAt(index, (int)length));
            }
            catch (CryptographicException ex) // RSA.DecryptBlock()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            return false;
        }
        private async Task<bool> ReceivePacketAsync_AES_256_CBC_SP()
        {
            CryptoAlgorithm alg = CryptoAlgorithm.AES_256_CBC_SP;
            try
            {
                int index = 1;
                byte[] ciphertext = new byte[16];
                if (!await parent.Channel.ReceiveAsync(ciphertext, 0, 16))
                    return false;
                byte[] plaintext = AesStatic.Decrypt(ciphertext, AesKey, ReceiveIV); //CryptographicException
                byte id = plaintext[0]; // index = 1

                bool isInternal = parent.Handler.IsInternalPacket(id);
                PacketRule rule = default(PacketRule);
                if (isInternal && !parent.Handler.ValidatePacket(id, alg, out rule))
                    return false;

                uint length = 0;
                if (isInternal && rule.Packet.ConstantLength.HasValue)
                    length = rule.Packet.ConstantLength.Value;
                else
                {
                    length = BitConverter.ToUInt32(plaintext, index);
                    index += 4;
                    if (!AssertSize(parent.Settings.MaxPacketSize, (int)length, nameof(ReceivePacketAsync_AES_256_CBC_SP)))
                        return false;
                }
                plaintext = plaintext.Skip(index);
                if (length > plaintext.Length - 2) // 2 random bytes
                {
                    int pendingLength = Convert.ToInt32(length - plaintext.Length + 2);
                    int pendingBlocks = Convert.ToInt32(Math.Ceiling((pendingLength + 1) / 16d)); // round up, first blocks only 15 bytes (padding)
                    ciphertext = new byte[pendingBlocks * 16];
                    if (!await parent.Channel.ReceiveAsync(ciphertext, 0, pendingBlocks * 16))
                        return false;
                    plaintext = Util.ConcatBytes(plaintext, AesStatic.Decrypt(ciphertext, AesKey, ReceiveIV));
                }
                int startIndex = Convert.ToInt32(plaintext.Length - length);
                byte[] content = plaintext.Skip(startIndex); // remove random bytes
                if (isInternal)
                {
#if DEBUG
                    parent.Log($"Received internal insecure AES packet: ID={id} Length={content.Length}");
#endif
                    return await parent.Handler.HandleInternalPacketAsync(rule, content);
                }
                else
                {
#if DEBUG
                    parent.Log($"Received external insecure AES packet: ID={255 - id} Length={content.Length}");
#endif
                    parent.OnPacketReceived(id, content);
                    return true;
                }
            }
            catch (CryptographicException ex) // AES.Decrypt()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            return false;
        }
        private async Task<bool> ReceivePacketAsync_AES_256_CBC_HMAC_SHA_256_MP3()
        {
            CryptoAlgorithm alg = CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3;
            try
            {
                byte[] buffer = new byte[35];
                if (!await parent.Channel.ReceiveAsync(buffer, 0, 35)) // 3 (length) + 32 (HMAC)
                    return false;
                int blocks = (int)UInt24.FromBytes(buffer, 0);
                byte[] hmac = buffer.Skip(3);
                const int defaultOverhead = 16 + 1 + 4; // iv + id + len
                int pendingLength = (blocks + 2) * 16; // at least one block; inclusive iv
                if (!AssertSize(parent.Settings.MaxPacketSize + defaultOverhead, pendingLength, nameof(ReceivePacketAsync_AES_256_CBC_HMAC_SHA_256_MP3)))
                    return false;
                byte[] cipherblock = new byte[pendingLength];
                if (!await parent.Channel.ReceiveAsync(cipherblock, 0, pendingLength))
                    return false;
                if (!hmac.SafeEquals(hmacProvider.ComputeHash(cipherblock)))
                {
                    parent.ExceptionHandler.CloseConnection("MessageCorrupted",
                        "The integrity checking resulted in a corrupted message.",
                        nameof(NetworkManager), nameof(ReceivePacketAsync_AES_256_CBC_HMAC_SHA_256_MP3));
                    return false;
                }
                byte[] iv = cipherblock.Take(16);
                byte[] ciphertext = cipherblock.Skip(16);
                using (PacketBuffer plaintext = PacketBuffer.CreateStatic(AesStatic.Decrypt(ciphertext, AesKey, iv)))
                {
                    while (plaintext.Position < plaintext.Length - 1)
                    {
                        byte id = plaintext.ReadByte();
                        bool isInternal = parent.Handler.IsInternalPacket(id);
                        PacketRule rule = default(PacketRule);
                        if (isInternal && !parent.Handler.ValidatePacket(id, alg, out rule))
                            return false;
                        uint length = rule.Packet?.ConstantLength ?? plaintext.ReadUInt();
                        if (length > plaintext.Pending)
                        {
                            parent.ExceptionHandler.CloseConnection("TooBigPacket",
                                $"Tried to receive a packet with {length} bytes length although only {plaintext.Pending} bytes are available.",
                        nameof(NetworkManager), nameof(ReceivePacketAsync_AES_256_CBC_HMAC_SHA_256_MP3));
                            return false;
                        }
                        byte[] content = plaintext.ReadByteArray((int)length);
                        if (isInternal)
                        {
#if DEBUG
                            parent.Log($"Received internal AES packet: ID={id} Length={content.Length}");
#endif
                            if (!await parent.Handler.HandleInternalPacketAsync(rule, content))
                                return false;
                        }
                        else
                        {
#if DEBUG
                            parent.Log($"Received external AES packet: ID={255 - id} Length={content.Length}");
#endif
                            parent.OnPacketReceived(id, content);
                        }
                    }
                }
            }
            catch (CryptographicException ex) // AES.Decrypt()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            return true;
        }
        #endregion receive
        #region send
        internal Task<bool> SendPacketAsync(byte id, byte[] content)
        {
            CryptoAlgorithm alg = VersionManager.GetNetworkAlgorithm(parent.ConnectionVersion);
            return SendPacketAsync(alg, id, true, content);
        }
        internal Task<bool> SendPacketAsync(Packet.IPacket packet)
        {
            CryptoAlgorithm alg = VersionManager.GetNetworkAlgorithm(parent.ConnectionVersion);
            return SendPacketAsync(alg, packet);
        }
        internal Task<bool> SendPacketAsync(CryptoAlgorithm alg, Packet.IPacket packet)
        {
            byte[] content;
            using (PacketBuffer buf = PacketBuffer.CreateDynamic())
            {
                packet.WritePacket(buf);
                content = buf.ToArray();
            }
            return SendPacketAsync(alg, packet.PacketId, !packet.ConstantLength.HasValue, content);
        }
        private async Task<bool> SendPacketAsync(CryptoAlgorithm alg, byte realId, bool size, byte[] content)
        {
            bool success;
            switch (alg)
            {
                case CryptoAlgorithm.None:
                    success = await SendPacketAsync_Plaintext(realId, size, content);
                    break;
                case CryptoAlgorithm.RSA_2048_OAEP:
                    success = await SendPacketAsync_RSA_2048_OAEP(realId, size, content);
                    break;
                case CryptoAlgorithm.AES_256_CBC_SP:
                    success = await SendPacketAsync_AES_256_CBC_SP(realId, size, content);
                    break;
                case CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3:
                    success = await SendPacketAsync_AES_256_CBC_HMAC_SHA256_MP3(realId, size, content);
                    break;
                default:
                    throw new ArgumentException("Unknown CryptoAlgorithm", nameof(alg));
            }
#if DEBUG
            string @namespace = realId < Constants.InternalPacketCount ? "internal" : "external";
            string prefix = success ? "Sent" : "Failed to send";
            byte displayId = realId < Constants.InternalPacketCount ? realId : (byte)(255 - realId);
            parent.Log($"{prefix} {@namespace} packet: ID={displayId} Length={content.Length} Algorithm={alg}");
#endif
            return success;
        }
        private Task<bool> SendPacketAsync_Plaintext(byte realId, bool size, byte[] content)
        {
            int length = 2 + (size ? 4 : 0) + content.Length;
            byte[] buf;
            using (PacketBuffer pbuf = PacketBuffer.CreateStatic(length))
            {
                pbuf.WriteByte((byte)CryptoAlgorithm.None);
                pbuf.WriteByte(realId);
                if (size) pbuf.WriteUInt((uint)content.Length);
                pbuf.WriteByteArray(content, false);
                buf = pbuf.ToArray();
            }
            return parent.Channel.SendAsync(buf, 0, buf.Length);
        }
        private Task<bool> SendPacketAsync_RSA_2048_OAEP(byte realId, bool size, byte[] content)
        {
            try
            {
                int length = 1 + (size ? 4 : 0) + content.Length;
                byte[] plain;
                using (PacketBuffer pbuf = PacketBuffer.CreateStatic(length))
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
                return parent.Channel.SendAsync(buf, 0, buf.Length);
            }
            catch (CryptographicException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return Task.FromResult(false);
            }
        }
        private Task<bool> SendPacketAsync_AES_256_CBC_SP(byte realId, bool size, byte[] content)
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
                using (PacketBuffer pbuf = PacketBuffer.CreateStatic(headLength + saltLength + content.Length))
                {
                    pbuf.WriteByte(realId);
                    if (size) pbuf.WriteUInt((uint)content.Length);
                    pbuf.WriteByteArray(salt, false);
                    pbuf.WriteByteArray(content, false);
                    plaintext = pbuf.ToArray();
                }
                byte[] headBlock = AesStatic.Encrypt(plaintext.Take(15), AesKey, SendIV);
                byte[] tailBlock = new byte[0];
                if (plaintext.Length > 15)
                {
                    plaintext = plaintext.Skip(15); // skip done bytes
                    tailBlock = AesStatic.Encrypt(plaintext, AesKey, SendIV); // encrypt remaining data
                }
                byte[] buf;
                using (PacketBuffer pbuf = PacketBuffer.CreateStatic(1 + headBlock.Length + tailBlock.Length))
                {
                    pbuf.WriteByte((byte)CryptoAlgorithm.AES_256_CBC_SP);
                    pbuf.WriteByteArray(headBlock, false);
                    pbuf.WriteByteArray(tailBlock, false);
                    buf = pbuf.ToArray();
                }
                return parent.Channel.SendAsync(buf, 0, buf.Length);
            }
            catch (CryptographicException ex) //Invalid key/iv
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return Task.FromResult(false);
            }
        }
        private Task<bool> SendPacketAsync_AES_256_CBC_HMAC_SHA256_MP3(byte realId, bool size, byte[] content)
        {
            int headLength = 1 + (size ? 4 : 0) + content.Length;
            byte[] plaintext;
            using (PacketBuffer pbuf = PacketBuffer.CreateStatic(headLength))
            {
                pbuf.WriteByte(realId);
                if (size) pbuf.WriteUInt((uint)content.Length);
                pbuf.WriteByteArray(content, false);
                plaintext = pbuf.ToArray();
            }
            byte[] iv = AesStatic.GenerateIV();
            byte[] ciphertext = AesStatic.Encrypt(plaintext, AesKey, iv);
            byte[] blocks = UInt24.ToBytes((uint)ciphertext.Length / 16 - 1);
            byte[] cipherblock = Util.ConcatBytes(iv, ciphertext);
            byte[] hmac = hmacProvider.ComputeHash(cipherblock);
            byte[] buf;
            using (PacketBuffer pbuf = PacketBuffer.CreateStatic(1 + 3 + hmac.Length + cipherblock.Length))
            {
                pbuf.WriteByte((byte)CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3);
                pbuf.WriteByteArray(blocks, false);
                pbuf.WriteByteArray(hmac, false);
                pbuf.WriteByteArray(cipherblock, false);
                buf = pbuf.ToArray();
            }
            return parent.Channel.SendAsync(buf, 0, buf.Length);
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
            HmacKey = Util.ConcatBytes(SendIV, ReceiveIV);
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
        #region exception
        /// <summary>
        /// Ensures that a packet id is only used for internal purposes.
        /// </summary>
        private bool AssertInternal(byte id, CryptoAlgorithm alg, string member)
        {
            bool isInternal = parent.Handler.IsInternalPacket(id);
            if (!isInternal) parent.ExceptionHandler.CloseConnection("InvalidPacket",
                $"Only internal packets are allowed to use {alg}.",
                nameof(NetworkManager), member);
            return isInternal;
        }

        /// <summary>
        /// Ensures that a packet size inside the maximum bounds.
        /// </summary>
        private bool AssertSize(int maximum, int actual, string member)
        {
            bool valid = actual <= maximum;
            if (!valid) parent.ExceptionHandler.CloseConnection("TooBigPacket",
                $"Tried to receive a packet of {actual} bytes. Maximum admissible are {maximum} bytes.",
                nameof(NetworkManager), member);
            return valid;
        }
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