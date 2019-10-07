using System;
using System.Runtime.CompilerServices;
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
        private readonly VSLSocket parent;
        private readonly SocketSettings settings;
        private readonly NetworkChannel channel;
        private readonly PacketHandler handler;
        private readonly ExceptionHandler exceptionHandler;
        private readonly RSAParameters rsaKey;
        private readonly object writePacketLock;
        private HMACSHA256 hmacProvider;
        internal volatile bool Ready4Aes = false;
        //  fields>
        // <constructor
        internal NetworkManager(VSLSocket parent, RSAParameters rsaKey)
        {
            this.parent = parent;
            this.settings = parent.Settings;
            this.channel = parent.Channel;
            this.handler = parent.Handler;
            this.exceptionHandler = parent.ExceptionHandler;
            this.rsaKey = rsaKey;
            writePacketLock = new object();
        }
        //  constructor>
        // <functions
        #region receive
        internal async Task<bool> ReceivePacketAsync()
        {
            byte[] buffer = new byte[1];
            if (!await channel.ReceiveAsync(buffer, 0, 1))
                return false;
            CryptoAlgorithm algorithm = (CryptoAlgorithm)buffer[0];
            switch (algorithm)
            {
                case CryptoAlgorithm.None:
                    return await ReceivePacketAsync_Plaintext();

                case CryptoAlgorithm.RSA_2048_OAEP:
                    return await ReceivePacketAsync_RSA_2048_OAEP();

                case CryptoAlgorithm.AES_256_CBC_SP:
                    if (!AssertKeyExchanged(CryptoAlgorithm.AES_256_CBC_SP) || !AssertAlgorithm(algorithm)) return false;
                    return await ReceivePacketAsync_AES_256_CBC_SP();

                case CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3:
                    if (!AssertKeyExchanged(CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3) || !AssertAlgorithm(algorithm)) return false;
                    return await ReceivePacketAsync_AES_256_CBC_HMAC_SHA_256_MP3();

                case CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_CTR:
                    if (!AssertKeyExchanged(CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_CTR) || !AssertAlgorithm(algorithm)) return false;
                    return await ReceivePacketAsync_AES_256_CBC_HMAC_SHA_256_CTR();

                default:
                    exceptionHandler.CloseConnection("InvalidAlgorithm",
                        $"Received packet with unknown algorithm ({algorithm}).",
                        nameof(NetworkManager));
                    return false;
            }

        }
        private async Task<bool> ReceivePacketAsync_Plaintext()
        {
            CryptoAlgorithm alg = CryptoAlgorithm.None;
            byte id; // read packet id
            byte[] buffer = new byte[1];
            if (!await channel.ReceiveAsync(buffer, 0, 1))
                return false;
            id = buffer[0];

            if (!AssertInternal(id, alg) || !handler.ValidatePacket(id, alg, out PacketRule rule))
                return false;

            uint length; // read packet length
            if (rule.Packet.ConstantLength.HasValue)
                length = rule.Packet.ConstantLength.Value;
            else
            {
                buffer = new byte[4];
                if (!await channel.ReceiveAsync(buffer, 0, 4))
                    return false;
                length = BitConverter.ToUInt32(buffer, 0);
                if (!AssertSize(settings.MaxPacketSize, (int)length))
                    return false;
            }

            buffer = new byte[length];
            if (!await channel.ReceiveAsync(buffer, 0, (int)length)) // read packet content
                return false;
#if DEBUG
            parent.Log($"Received internal plaintext packet: ID={id} Length={length}");
#endif
            return await handler.HandleInternalPacketAsync(rule, buffer);
        }
        private async Task<bool> ReceivePacketAsync_RSA_2048_OAEP()
        {
            CryptoAlgorithm alg = CryptoAlgorithm.RSA_2048_OAEP;
            int index = 1;
            byte[] ciphertext = new byte[256];
            if (!await channel.ReceiveAsync(ciphertext, 0, 256))
                return false;
            byte[] plaintext;
            try
            {
                plaintext = RsaStatic.DecryptBlock(ciphertext, rsaKey);
            }
            catch (CryptographicException ex)
            {
                exceptionHandler.CloseConnection(ex);
                return false;
            }
            byte id = plaintext[0]; // index = 1
            if (!AssertInternal(id, alg) || !handler.ValidatePacket(id, alg, out PacketRule rule))
                return false;

            uint length;
            if (rule.Packet.ConstantLength.HasValue)
                length = rule.Packet.ConstantLength.Value;
            else
            {
                length = BitConverter.ToUInt32(plaintext, index);
                index += 4;
                if (!AssertSize(209, (int)length))
                    return false; // 214 - 1 (id) - 4 (uint) => 209
            }
            if (!AssertAvailable(plaintext.Length - index, (int)length))
                return false;
            parent.Log($"Received internal RSA packet: ID={id} Length={length}");
            return await handler.HandleInternalPacketAsync(rule, plaintext.TakeAt(index, (int)length));
        }
        private async Task<bool> ReceivePacketAsync_AES_256_CBC_SP()
        {
            CryptoAlgorithm alg = CryptoAlgorithm.AES_256_CBC_SP;
            int index = 1;
            byte[] ciphertext = new byte[16];
            if (!await channel.ReceiveAsync(ciphertext, 0, 16))
                return false;
            if (!TryDecrypt(ciphertext, AesKey, ReceiveIV, out byte[] plaintext))
                return false;
            byte id = plaintext[0]; // index = 1

            bool isInternal = handler.IsInternalPacket(id);
            PacketRule rule = default;
            if (isInternal && !handler.ValidatePacket(id, alg, out rule))
                return false;

            uint length;
            if (isInternal && rule.Packet.ConstantLength.HasValue)
                length = rule.Packet.ConstantLength.Value;
            else
            {
                length = BitConverter.ToUInt32(plaintext, index);
                index += 4;
                if (!AssertSize(settings.MaxPacketSize, (int)length))
                    return false;
            }
            plaintext = plaintext.Skip(index);
            if (length > plaintext.Length - 2) // 2 random bytes
            {
                int pendingLength = Convert.ToInt32(length - plaintext.Length + 2);
                int pendingBlocks = Convert.ToInt32(Math.Ceiling((pendingLength + 1) / 16d)); // round up, first blocks only 15 bytes (padding)
                ciphertext = new byte[pendingBlocks * 16];
                if (!await channel.ReceiveAsync(ciphertext, 0, pendingBlocks * 16))
                    return false;
                if (!TryDecrypt(ciphertext, AesKey, ReceiveIV, out byte[] tail))
                    return false;
                plaintext = Util.ConcatBytes(plaintext, tail);
            }
            if (!AssertAvailable(plaintext.Length, (int)length))
                return false;
            int startIndex = plaintext.Length - (int)length;
            byte[] content = plaintext.Skip(startIndex); // remove random bytes
            if (isInternal)
            {
                parent.Log($"Received internal insecure AES packet: ID={id} Length={content.Length}");
                return await handler.HandleInternalPacketAsync(rule, content);
            }
            else
            {
                parent.Log($"Received external insecure AES packet: ID={255 - id} Length={content.Length}");
                return await parent.OnPacketReceived(id, content);
            }
        }
        private async Task<bool> ReceivePacketAsync_AES_256_CBC_HMAC_SHA_256_MP3()
        {
            CryptoAlgorithm alg = CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3;
            byte[] buffer = new byte[35];
            if (!await channel.ReceiveAsync(buffer, 0, 35)) // 3 (length) + 32 (HMAC)
                return false;
            int blocks = (int)UInt24.FromBytes(buffer, 0);
            byte[] hmac = buffer.Skip(3);
            const int defaultOverhead = 16 + 1 + 4; // iv + id + len
            int pendingLength = (blocks + 2) * 16; // at least one block; inclusive iv
            if (!AssertSize(settings.MaxPacketSize + defaultOverhead, pendingLength))
                return false;
            byte[] cipherblock = new byte[pendingLength];
            if (!await channel.ReceiveAsync(cipherblock, 0, pendingLength))
                return false;
            if (!hmac.SafeEquals(hmacProvider.ComputeHash(cipherblock)))
            {
                exceptionHandler.CloseConnection("MessageCorrupted",
                    "The integrity checking resulted in a corrupted message.",
                    nameof(NetworkManager));
                return false;
            }
            byte[] iv = cipherblock.Take(16);
            byte[] ciphertext = cipherblock.Skip(16);
            if (!TryDecrypt(ciphertext, AesKey, iv, out byte[] plainbuffer))
                return false;
            using (PacketBuffer plaintext = PacketBuffer.CreateStatic(plainbuffer))
            {
                while (plaintext.Position < plaintext.Length - 1)
                {
                    byte id = plaintext.ReadByte();
                    bool isInternal = handler.IsInternalPacket(id);
                    PacketRule rule = default;
                    if (isInternal && !handler.ValidatePacket(id, alg, out rule))
                        return false;
                    uint length = rule.Packet?.ConstantLength ?? plaintext.ReadUInt();
                    if (!AssertAvailable(plaintext.Pending, (int)length))
                        return false;
                    byte[] content = plaintext.ReadByteArray((int)length);
                    if (isInternal)
                    {
                        parent.Log($"Received internal AES packet: ID={id} Length={content.Length}");
                        if (!await handler.HandleInternalPacketAsync(rule, content))
                            return false;
                    }
                    else
                    {
                        parent.Log($"Received external AES packet: ID={255 - id} Length={content.Length}");
                        if (!await parent.OnPacketReceived(id, content))
                            return false;
                    }
                }
            }
            return true;
        }
        private async Task<bool> ReceivePacketAsync_AES_256_CBC_HMAC_SHA_256_CTR()
        {
            const CryptoAlgorithm alg = CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_CTR;
            byte[] buffer = new byte[35];
            if (!await channel.ReceiveAsync(buffer, 0, 35)) // 3 (length) + 32 (HMAC)
                return false;
            int blocks = (int)UInt24.FromBytes(buffer, 0);
            byte[] hmac = buffer.Skip(3);
            int pendingLength = (blocks + 1) * 16; // at least one block, no iv
            if (!AssertSize(settings.MaxPacketSize + 1, pendingLength))
                return false;
            byte[] cipherblock = new byte[pendingLength];
            if (!await channel.ReceiveAsync(cipherblock, 0, pendingLength))
                return false;
            byte[] realHmac = hmacProvider.ComputeHash(cipherblock);
            if (!hmac.SafeEquals(realHmac))
            {
                exceptionHandler.CloseConnection("MessageCorrupted",
                    "The integrity checking resulted in a corrupted message.",
                    nameof(NetworkManager));
                return false;
            }
            if (!TryDecrypt(cipherblock, AesKey, ReceiveIV, out byte[] plaintext))
                return false;
            AesStatic.IncrementIV(ReceiveIV);
            if (plaintext.Length < 1)
            {
                exceptionHandler.CloseConnection("TooShortPacket",
                    "The packet's plaintext must contain at least one byte as ID.",
                    nameof(NetworkManager));
                return false;
            }
            byte id = plaintext[0];
            bool isInternal = handler.IsInternalPacket(id);
            PacketRule rule = default;
            if (isInternal && !handler.ValidatePacket(id, alg, out rule))
                return false;
            byte[] content = plaintext.TakeAt(1, plaintext.Length - 1);
            if (isInternal)
            {
                parent.Log($"Received internal AES packet: ID={id} Length={content.Length}");
                return await handler.HandleInternalPacketAsync(rule, content);
            }
            else
            {
                parent.Log($"Received external AES packet: ID={255 - id} Length={content.Length}");
                return await parent.OnPacketReceived(id, content);
            }
        }
        #endregion receive
        #region send
        internal Task<bool> SendPacketAsync(byte id, byte[] content)
        {
            CryptoAlgorithm alg = VersionManager.GetNetworkAlgorithm(parent.ConnectionVersion);
            return SendPacketAsync(alg, id, true, content, false);
        }
        internal Task<bool> SendPacketAsync(Packet.IPacket packet, bool background = false)
        {
            CryptoAlgorithm alg = VersionManager.GetNetworkAlgorithm(parent.ConnectionVersion);
            return SendPacketAsync(alg, packet, background);
        }
        internal Task<bool> SendPacketAsync(CryptoAlgorithm alg, Packet.IPacket packet, bool background = false)
        {
            byte[] content;
            using (PacketBuffer buf = PacketBuffer.CreateDynamic())
            {
                packet.WritePacket(buf);
                content = buf.ToArray();
            }
            return SendPacketAsync(alg, packet.PacketId, !packet.ConstantLength.HasValue, content, background);
        }
        private Task<bool> SendPacketAsync(CryptoAlgorithm alg, byte id, bool size, byte[] content, bool background)
        {
            lock (writePacketLock) // Some calls like IncrementIV or HMAC are not threadsafe
            {
                byte[] buffer;
                switch (alg)
                {
                    case CryptoAlgorithm.None:
                        buffer = WritePacket_Plaintext(id, size, content);
                        break;
                    case CryptoAlgorithm.RSA_2048_OAEP:
                        buffer = WritePacket_RSA_2048_OAEP(id, size, content);
                        break;
                    case CryptoAlgorithm.AES_256_CBC_SP:
                        buffer = WritePacket_AES_256_CBC_SP(id, size, content);
                        break;
                    case CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3:
                        buffer = WritePacket_AES_256_CBC_HMAC_SHA256_MP3(id, size, content);
                        break;
                    case CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_CTR:
                        buffer = WritePacket_AES_256_CBC_HMAC_SHA256_CTR(id, content);
                        break;
                    default:
                        throw new ArgumentException("Unknown CryptoAlgorithm", nameof(alg));
                }
#if DEBUG
                string @namespace = id < Constants.InternalPacketCount ? "internal" : "external";
                byte displayId = id < Constants.InternalPacketCount ? id : (byte)(255 - id);
                parent.Log($"Sending {@namespace} packet: ID={displayId} Length={content.Length} Algorithm={alg}");
#endif
                return channel.SendAsync(buffer, 0, buffer.Length, background);
            }
        }
        private byte[] WritePacket_Plaintext(byte id, bool size, byte[] content)
        {
            int length = 2 + (size ? 4 : 0) + content.Length;
            using (PacketBuffer pbuf = PacketBuffer.CreateStatic(length))
            {
                pbuf.WriteByte((byte)CryptoAlgorithm.None);
                pbuf.WriteByte(id);
                if (size) pbuf.WriteUInt((uint)content.Length);
                pbuf.WriteByteArray(content, false);
                return pbuf.ToArray();
            }
        }
        private byte[] WritePacket_RSA_2048_OAEP(byte id, bool size, byte[] content)
        {
            int length = 1 + (size ? 4 : 0) + content.Length;
            byte[] plain;
            using (PacketBuffer pbuf = PacketBuffer.CreateStatic(length))
            {
                pbuf.WriteByte(id);
                if (size) pbuf.WriteUInt((uint)content.Length);
                pbuf.WriteByteArray(content, false);
                plain = pbuf.ToArray();
            }
            byte[] ciphertext = RsaStatic.EncryptBlock(plain, rsaKey);
            byte[] buf = new byte[1 + ciphertext.Length];
            buf[0] = (byte)CryptoAlgorithm.RSA_2048_OAEP;
            Array.Copy(ciphertext, 0, buf, 1, ciphertext.Length);
            return buf;
        }
        private byte[] WritePacket_AES_256_CBC_SP(byte id, bool size, byte[] content)
        {
            int headLength = 1 + (size ? 4 : 0);
            int saltLength;
            if (headLength + 2 + content.Length < 16) // 2 random bytes
                saltLength = 15 - headLength - content.Length; // 16 - 1 (padding) = 15
            else
            {
                int blocks = Util.GetTotalSize(headLength + 4 + content.Length, 16) / 16; // 2 random bytes + 2x padding
                saltLength = blocks * 16 - headLength - content.Length - 2; // 2x padding
            }
            byte[] salt = new byte[saltLength];
            Random random = new Random();
            random.NextBytes(salt);
            byte[] plaintext;
            using (PacketBuffer pbuf = PacketBuffer.CreateStatic(headLength + saltLength + content.Length))
            {
                pbuf.WriteByte(id);
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
            using (PacketBuffer pbuf = PacketBuffer.CreateStatic(1 + headBlock.Length + tailBlock.Length))
            {
                pbuf.WriteByte((byte)CryptoAlgorithm.AES_256_CBC_SP);
                pbuf.WriteByteArray(headBlock, false);
                pbuf.WriteByteArray(tailBlock, false);
                return pbuf.ToArray();
            }
        }
        private byte[] WritePacket_AES_256_CBC_HMAC_SHA256_MP3(byte id, bool size, byte[] content)
        {
            int headLength = 1 + (size ? 4 : 0) + content.Length;
            byte[] plaintext;
            using (PacketBuffer pbuf = PacketBuffer.CreateStatic(headLength))
            {
                pbuf.WriteByte(id);
                if (size) pbuf.WriteUInt((uint)content.Length);
                pbuf.WriteByteArray(content, false);
                plaintext = pbuf.ToArray();
            }
            byte[] iv = AesStatic.GenerateIV();
            byte[] ciphertext = AesStatic.Encrypt(plaintext, AesKey, iv);
            byte[] blocks = UInt24.ToBytes((uint)ciphertext.Length / 16 - 1);
            byte[] cipherblock = Util.ConcatBytes(iv, ciphertext);
            byte[] hmac = hmacProvider.ComputeHash(cipherblock);
            using (PacketBuffer pbuf = PacketBuffer.CreateStatic(1 + 3 + hmac.Length + cipherblock.Length))
            {
                pbuf.WriteByte((byte)CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3);
                pbuf.WriteByteArray(blocks, false);
                pbuf.WriteByteArray(hmac, false);
                pbuf.WriteByteArray(cipherblock, false);
                return pbuf.ToArray();
            }
        }
        private byte[] WritePacket_AES_256_CBC_HMAC_SHA256_CTR(byte id, byte[] content)
        {
            byte[] plaintext;
            using (PacketBuffer pbuf = PacketBuffer.CreateStatic(1 + content.Length))
            {
                pbuf.WriteByte(id);
                pbuf.WriteByteArray(content, false);
                plaintext = pbuf.ToArray();
            }
            ArraySegment<byte> ciphertext = AesStatic.Encrypt(new ArraySegment<byte>(plaintext), AesKey, SendIV);
            AesStatic.IncrementIV(SendIV);
            byte[] blocks = UInt24.ToBytes((uint)ciphertext.Count / 16 - 1);
            byte[] hmac = hmacProvider.ComputeHash(ciphertext.Array, ciphertext.Offset, ciphertext.Count);
            using (PacketBuffer pbuf = PacketBuffer.CreateStatic(1 + 3 + hmac.Length + ciphertext.Count))
            {
                pbuf.WriteByte((byte)CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_CTR);
                pbuf.WriteByteArray(blocks, false);
                pbuf.WriteByteArray(hmac, false);
                pbuf.WriteByteArray(ciphertext, false);
                return pbuf.ToArray();
            }
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
        /// Ensures that cryptographic keys have been exchanged.
        /// </summary>
        private bool AssertKeyExchanged(CryptoAlgorithm algorithm, [CallerMemberName]string member = Constants.DefaultMemberName)
        {
            if (!Ready4Aes)
            {
                exceptionHandler.CloseConnection("InvalidOperation",
                    $"Not ready to receive a packet with CryptoAlgorithm.{algorithm}, because key exchange is not finished yet",
                    nameof(NetworkManager), member);
            }
            return Ready4Aes;
        }

        /// <summary>
        /// Ensures that the correct <see cref="CryptoAlgorithm"/> is used.
        /// </summary>
        private bool AssertAlgorithm(CryptoAlgorithm alg, [CallerMemberName]string member = Constants.DefaultMemberName)
        {
            if (parent.ConnectionVersion.HasValue && alg != VersionManager.GetNetworkAlgorithm(parent.ConnectionVersion))
            {
                exceptionHandler.CloseConnection("InvalidAlgorithm",
                    $"VSL version {parent.ConnectionVersionString} should not use {alg}",
                    nameof(NetworkManager), member);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Ensures that a packet id is only used for internal purposes.
        /// </summary>
        private bool AssertInternal(byte id, CryptoAlgorithm alg, [CallerMemberName]string member = Constants.DefaultMemberName)
        {
            bool isInternal = handler.IsInternalPacket(id);
            if (!isInternal) exceptionHandler.CloseConnection("InvalidPacket",
                $"Only internal packets are allowed to use {alg}",
                nameof(NetworkManager), member);
            return isInternal;
        }

        /// <summary>
        /// Ensures that a packet size inside the maximum bounds.
        /// </summary>
        private bool AssertSize(int maximum, int actual, [CallerMemberName]string member = Constants.DefaultMemberName)
        {
            bool valid = actual <= maximum;
            if (!valid) exceptionHandler.CloseConnection("TooBigPacket",
                $"Tried to receive a packet of {actual} bytes. Maximum admissible are {maximum} bytes",
                nameof(NetworkManager), member);
            return valid;
        }

        /// <summary>
        /// Ensures that the packet length marker is valid.
        /// </summary>
        private bool AssertAvailable(int maximum, int actual, [CallerMemberName]string member = Constants.DefaultMemberName)
        {
            bool valid = actual <= maximum;
            if (!valid) exceptionHandler.CloseConnection("TooBigPacket",
                $"Tried to receive a packet with {actual} bytes length although only {maximum} bytes are available.",
                nameof(NetworkManager), member);
            return valid;
        }

        /// <summary>
        /// Exception catching wrapper for <see cref="AesStatic.Decrypt(byte[], byte[], byte[])"/>
        /// </summary>
        private bool TryDecrypt(byte[] buffer, byte[] key, byte[] iv, out byte[] plaintext)
        {
            try
            {
                plaintext = AesStatic.Decrypt(buffer, key, iv);
                return true;
            }
            catch (CryptographicException ex)
            {
                exceptionHandler.CloseConnection(ex);
                plaintext = null;
                return false;
            }
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
                    hmacProvider?.Dispose();
                }

                disposedValue = true;
            }
        }

        // ~NetworkManager() {
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }
        #endregion
        //  functions>
    }
}