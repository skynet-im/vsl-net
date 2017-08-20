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
    internal abstract class NetworkManager : IDisposable
    {
        // <fields
        internal VSLSocket parent;
        internal bool Ready4Aes = false;
        private AesCsp enc;
        private AesCsp dec;
        //  fields>
        // <constructor
        internal void InitializeComponent()
        {

        }
        //  constructor>
        // <functions
        #region receive
        internal void OnDataReceive()
        {
            try
            {
                byte b = parent.channel.Read(1)[0];
                CryptographicAlgorithm algorithm = (CryptographicAlgorithm)b;
                switch (algorithm)
                {
                    case CryptographicAlgorithm.None:
                        ReceivePacket_Plaintext();
                        break;
                    case CryptographicAlgorithm.RSA_2048:
                        ReceivePacket_RSA_2048();
                        break;
                    case CryptographicAlgorithm.AES_256:
                        if (Ready4Aes)
                            ReceivePacket_AES_256();
                        else
                            throw new InvalidOperationException("Not ready to receive an AES packet, because key exchange is not finished yet.");
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Received packet with unknown algorithm ({0})", algorithm.ToString()));
                }
            }
            catch (InvalidCastException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (InvalidOperationException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (OperationCanceledException) // NetworkChannel.Read()
            {
                // Already shutting down...
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
        }
        private void ReceivePacket_Plaintext()
        {
            try
            {
                byte id = parent.channel.Read(1)[0];
                bool success = parent.handler.TryGetPacket(id, out Packet.IPacket packet);
                if (success)
                {
                    uint length = 0;
                    if (packet.PacketLength.Type == Packet.PacketLength.LengthType.Constant)
                    {
                        length = packet.PacketLength.Length;
                    }
                    else if (packet.PacketLength.Type == Packet.PacketLength.LengthType.UInt32)
                    {
                        length = BitConverter.ToUInt32(parent.channel.Read(4), 0);
                        if (length > Constants.MaxPacketSize)
                            throw new System.IO.InvalidDataException(string.Format("Tried to receive a packet of {0} bytes. Maximum admissible are {1} bytes", length, Constants.MaxPacketSize));
                    }
                    parent.handler.HandleInternalPacket(id, parent.channel.Read(Convert.ToInt32(length)));
                }
                else
                {
                    throw new InvalidOperationException("Unknown packet id " + id);
                }
            }
            catch (ArgumentOutOfRangeException ex) // IPacket.ReadPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (InvalidCastException ex) // PacketHandler.HandleInternalPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (System.IO.InvalidDataException ex) // Too big packet
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (InvalidOperationException ex) // PacketHandler.HandleInternalPacket() and unkown packet
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotImplementedException ex) // PacketHandler.HandleInternalPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotSupportedException ex) // PacketHandler.HandleInternalPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (OperationCanceledException) // NetworkChannel.Read()
            {
                // Already shutting down...
            }
            catch (TimeoutException ex) // NetworkChannel.Read()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
        }
        private void ReceivePacket_RSA_2048()
        {
            try
            {
                int index = 1;
                byte[] ciphertext = parent.channel.Read(256);
                byte[] plaintext = RSA.DecryptBlock(ciphertext, Keypair);
                byte id = plaintext[0]; // index = 1
                bool success = parent.handler.TryGetPacket(id, out Packet.IPacket packet);
                if (success)
                {
                    uint length = 0;
                    if (packet.PacketLength.Type == Packet.PacketLength.LengthType.Constant)
                    {
                        length = packet.PacketLength.Length;
                    }
                    else if (packet.PacketLength.Type == Packet.PacketLength.LengthType.UInt32)
                    {
                        length = BitConverter.ToUInt32(Util.TakeBytes(plaintext, 4, index), 0);
                        if (length > 251)
                            throw new System.IO.InvalidDataException(string.Format("Tried to receive a packet of {0} bytes. Maximum admissible are 251 bytes", length));
                        index += 4;
                    }
                    parent.handler.HandleInternalPacket(id, Util.TakeBytes(plaintext, Convert.ToInt32(length), index));
                }
                else
                {
                    throw new InvalidOperationException("Unknown packet id " + id);
                }
            }
            catch (ArgumentOutOfRangeException ex) // IPacket.ReadPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (System.Security.Cryptography.CryptographicException ex) // RSA.DecryptBlock()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (InvalidCastException ex) // PacketHandler.HandleInternalPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (System.IO.InvalidDataException ex) // Too big packet
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (InvalidOperationException ex) // PacketHandler.HandleInternalPacket() and unkown packet
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotImplementedException ex) // PacketHandler.HandleInternalPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotSupportedException ex) // PacketHandler.HandleInternalPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (OperationCanceledException) // NetworkChannel.Read()
            {
                // Already shutting down...
            }
            catch (TimeoutException ex) // NetworkChannel.Read()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
        }
        private void ReceivePacket_AES_256()
        {
            try
            {
                int index = 1;
                byte[] ciphertext = parent.channel.Read(16); //TimeoutException
                byte[] plaintext = dec.Decrypt(ciphertext); //CryptographicException
                byte id = plaintext[0];
                bool success = parent.handler.TryGetPacket(id, out Packet.IPacket packet);
                uint length = 0;
                if (success && packet.PacketLength.Type == Packet.PacketLength.LengthType.Constant)
                {
                    length = packet.PacketLength.Length;
                }
                else
                {
                    length = BitConverter.ToUInt32(Util.TakeBytes(plaintext, 4, index), 0);
                    index += 4;
                    if (length > Constants.MaxPacketSize)
                        throw new System.IO.InvalidDataException(string.Format("Tried to receive a packet of {0} bytes. Maximum admissible are {1} bytes", length, Constants.MaxPacketSize));
                }
                plaintext = Util.SkipBytes(plaintext, index);
                if (length > plaintext.Length - 2) // 2 random bytes
                {
                    int pendingLength = Convert.ToInt32(length - plaintext.Length + 2);
                    int pendingBlocks = Convert.ToInt32(Math.Ceiling((pendingLength + 1) / 16d)); // round up, first blocks only 15 bytes (padding)
                    ciphertext = parent.channel.Read(pendingBlocks * 16);
                    plaintext = Util.ConnectBytesPA(plaintext, dec.Decrypt(ciphertext));
                }
                int startIndex = Convert.ToInt32(plaintext.Length - length);
                byte[] content = Util.SkipBytes(plaintext, startIndex); // remove random bytes
                if (success)
                {
                    parent.Logger.D(string.Format("Received internal AES packet: ID={0} Length={1}", id, content.Length));
                    parent.handler.HandleInternalPacket(id, content);
                }
                else
                {
                    parent.Logger.D(string.Format("Received external AES packet: ID={0} Length={1}", 255 - id, content.Length));
                    parent.OnPacketReceived(id, content);
                }
            }
            catch (ArgumentOutOfRangeException ex) // IPacket.ReadPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (System.Security.Cryptography.CryptographicException ex) // AES.Decrypt()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (InvalidCastException ex) // PacketHandler.HandleInternalPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (System.IO.InvalidDataException ex) // Too big packet
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (InvalidOperationException ex) // PacketHandler.HandleInternalPacket() and unkown packet
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotImplementedException ex) // PacketHandler.HandleInternalPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (NotSupportedException ex) // PacketHandler.HandleInternalPacket()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (OperationCanceledException) // NetworkChannel.Read()
            {
                // Already shutting down...
            }
            catch (TimeoutException ex) // NetworkChannel.Read()
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
        }
        #endregion receive
        #region send
        internal bool SendPacket(byte id, byte[] content)
        {
            byte[] head = Util.ConnectBytesPA(new byte[1] { id }, BitConverter.GetBytes(Convert.ToUInt32(content.Length)));
            return SendPacket(CryptographicAlgorithm.AES_256, head, content);
        }
        internal Task<bool> SendPacketAsync(byte id, byte[] content)
        {
            byte[] head = Util.ConnectBytesPA(new byte[1] { id }, BitConverter.GetBytes(Convert.ToUInt32(content.Length)));
            return SendPacketAsync(CryptographicAlgorithm.AES_256, head, content);
        }
        internal bool SendPacket(Packet.IPacket packet)
        {
            return SendPacket(CryptographicAlgorithm.AES_256, packet);
        }
        internal Task<bool> SendPacketAsync(Packet.IPacket packet)
        {
            return SendPacketAsync(CryptographicAlgorithm.AES_256, packet);
        }
        internal bool SendPacket(CryptographicAlgorithm alg, Packet.IPacket packet)
        {
            byte[] head = new byte[1] { packet.PacketID };
            PacketBuffer buf = new PacketBuffer();
            packet.WritePacket(buf);
            byte[] content = buf.ToArray();
            buf.Dispose();
            if (packet.PacketLength.Type == Packet.PacketLength.LengthType.UInt32)
                head = Util.ConnectBytesPA(head, BitConverter.GetBytes(Convert.ToUInt32(content.Length)));
            return SendPacket(alg, head, content);
        }
        internal Task<bool> SendPacketAsync(CryptographicAlgorithm alg, Packet.IPacket packet)
        {
            byte[] head = new byte[1] { packet.PacketID };
            PacketBuffer buf = new PacketBuffer();
            packet.WritePacket(buf);
            byte[] content = buf.ToArray();
            buf.Dispose();
            if (packet.PacketLength.Type == Packet.PacketLength.LengthType.UInt32)
                head = Util.ConnectBytesPA(head, BitConverter.GetBytes(Convert.ToUInt32(content.Length)));
            return SendPacketAsync(alg, head, content);
        }
        internal bool SendPacket(CryptographicAlgorithm alg, byte[] head, byte[] content)
        {
            switch (alg)
            {
                case CryptographicAlgorithm.None:
                    return SendPacket_Plaintext(head, content);
                case CryptographicAlgorithm.RSA_2048:
                    return SendPacket_RSA_2048(head, content);
                case CryptographicAlgorithm.AES_256:
                    return SendPacket_AES_256(head, content);
                default:
                    throw new InvalidOperationException();
            }
        }
        internal Task<bool> SendPacketAsync(CryptographicAlgorithm alg, byte[] head, byte[] content)
        {
            switch (alg)
            {
                case CryptographicAlgorithm.None:
                    return SendPacketAsync_Plaintext(head, content);
                case CryptographicAlgorithm.RSA_2048:
                    return SendPacketAsync_RSA_2048(head, content);
                case CryptographicAlgorithm.AES_256:
                    return SendPacketAsync_AES_256(head, content);
                default:
                    throw new InvalidOperationException();
            }
        }
        private bool SendPacket_Plaintext(byte[] head, byte[] content)
        {
            byte[] buf = Util.ConnectBytesPA(new byte[1] { (byte)CryptographicAlgorithm.None }, head, content);
            bool success = buf.Length == parent.channel.Send(buf);
            buf = null;
            return success;
        }
        private async Task<bool> SendPacketAsync_Plaintext(byte[] head, byte[] content)
        {
            byte[] buf = Util.ConnectBytesPA(new byte[1] { (byte)CryptographicAlgorithm.None }, head, content);
            bool success = buf.Length == await Task.Run(() => parent.channel.Send(buf));
            buf = null;
            return success;
        }
        private bool SendPacket_RSA_2048(byte[] head, byte[] content)
        {
            try
            {
                byte[] ciphertext = RSA.EncryptBlock(Util.ConnectBytesPA(head, content), PublicKey);
                byte[] buf = Util.ConnectBytesPA(new byte[1] { (byte)CryptographicAlgorithm.RSA_2048 }, ciphertext);
                bool success = buf.Length == parent.channel.Send(buf);
                ciphertext = null;
                buf = null;
                return success;
            }
            catch (System.Security.Cryptography.CryptographicException ex)
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
        private async Task<bool> SendPacketAsync_RSA_2048(byte[] head, byte[] content)
        {
            try
            {
                byte[] ciphertext = await Task.Run(() => RSA.EncryptBlock(Util.ConnectBytesPA(head, content), PublicKey));
                byte[] buf = Util.ConnectBytesPA(new byte[1] { (byte)CryptographicAlgorithm.RSA_2048 }, ciphertext);
                bool success = buf.Length == await Task.Run(() => parent.channel.Send(buf));
                ciphertext = null;
                buf = null;
                return success;
            }
            catch (System.Security.Cryptography.CryptographicException ex)
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
        private bool SendPacket_AES_256(byte[] head, byte[] content)
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
                byte[] plaintext = Util.ConnectBytesPA(head, salt, content);
                byte[] headBlock = enc.Encrypt(Util.TakeBytes(plaintext, 15));
                byte[] tailBlock = new byte[0];
                if (plaintext.Length > 15)
                {
                    plaintext = Util.SkipBytes(plaintext, 15);
                    tailBlock = enc.Encrypt(plaintext);
                }
                byte[] buf = Util.ConnectBytesPA(new byte[1] { (byte)CryptographicAlgorithm.AES_256 }, headBlock, tailBlock);
                bool success = buf.Length == parent.channel.Send(buf);
                if (head[0] <= 9)
                    parent.Logger.D(string.Format("Sent internal AES packet: ID={0} Length={1} {2}b", head[0], buf.Length, blocks));
                else
                    parent.Logger.D(string.Format("Sent external AES packet: ID={0} Length={1} {2}b", 255 - head[0], buf.Length, blocks));
                salt = null;
                plaintext = null;
                headBlock = null;
                tailBlock = null;
                return success;
            }
            catch (System.Security.Cryptography.CryptographicException ex) //Invalid key/iv
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
        }
        private async Task<bool> SendPacketAsync_AES_256(byte[] head, byte[] content)
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
                byte[] plaintext = Util.ConnectBytesPA(head, salt, content);
                byte[] headBlock = await enc.EncryptAsync(Util.TakeBytes(plaintext, 15));
                byte[] tailBlock = new byte[0];
                if (plaintext.Length > 15)
                {
                    plaintext = Util.SkipBytes(plaintext, 15);
                    tailBlock = await enc.EncryptAsync(plaintext);
                }
                byte[] buf = Util.ConnectBytesPA(new byte[1] { (byte)CryptographicAlgorithm.AES_256 }, headBlock, tailBlock);
                bool success = buf.Length == await Task.Run(() => parent.channel.Send(buf));
                if (head[0] <= 9)
                    parent.Logger.D(string.Format("Sent internal AES packet: ID={0} Length={1} {2}b", head[0], buf.Length, blocks));
                else
                    parent.Logger.D(string.Format("Sent external AES packet: ID={0} Length={1} {2}b", 255 - head[0], buf.Length, blocks));
                salt = null;
                plaintext = null;
                headBlock = null;
                tailBlock = null;
                return success;
            }
            catch (System.Security.Cryptography.CryptographicException ex) //Invalid key/iv
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
        }
        #endregion send
        internal abstract string PublicKey { get; }
        internal abstract string Keypair { get; }
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
                if (enc == null)
                    enc = new AesCsp(value);
                else
                    enc.Key = value;
                if (dec == null)
                    dec = new AesCsp(value);
                else
                    dec.Key = value;
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
                if (dec == null) throw new InvalidOperationException("You have to asign the key before the iv");
                _receiveIV = value;
                dec.IV = value;
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
                if (enc == null) throw new InvalidOperationException("You have to asign the key before the iv");
                _sendIV = value;
                enc.IV = value;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // -TODO: dispose managed state (managed objects).
                    enc?.Dispose();
                    dec?.Dispose();
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