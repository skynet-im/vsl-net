using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using VSL.FileTransfer;
using VSL.Packet;

namespace VSL
{
    /// <summary>
    /// A portable VSL client.
    /// </summary>
    public class VSLClient : IDisposable
    {
        // <fields
        internal bool ConnectionAvailable;
        internal NetworkChannel channel;
        internal NetworkManager manager;
        internal PacketHandler handler;
        /// <summary>
        /// Access file transfer functions.
        /// </summary>
        public FileTransferSocket FileTransfer;
        internal ExceptionHandler ExceptionHandler;
        /// <summary>
        /// Configure necessary console output.
        /// </summary>
        public Logger Logger;
        internal ushort LatestProduct;
        internal ushort OldestProduct;
        private int _networkBufferSize = Constants.ReceiveBufferSize;
        //  fields>
        // <constructor
        /// <summary>
        /// Creates a VSL Client that has to be connected
        /// </summary>
        /// <param name="latestProduct">The application version</param>
        /// <param name="oldestProduct">The oldest supported version</param>
        public VSLClient(ushort latestProduct, ushort oldestProduct)
        {
            LatestProduct = latestProduct;
            OldestProduct = oldestProduct;

            FileTransfer = new FileTransferSocket(this);
            ExceptionHandler = new ExceptionHandler(this);
            Logger = new Logger(this);
        }
        //  constructor>
        // <properties
        /// <summary>
        /// Gets or sets a value that specifies the size of the receive buffer of the Socket.
        /// </summary>
        public int ReceiveBufferSize
        {
            get
            {
                if (channel != null)
                    return channel.NetworkBufferSize;
                else
                    return _networkBufferSize;
            }
            set
            {
                _networkBufferSize = value;
                if (channel != null)
                    channel.NetworkBufferSize = value;
            }
        }
        //  properties>
        // <events
        /// <summary>
        /// The ConnectionEstablished event occurs when the connection was build up and the key exchange was finished
        /// </summary>
        public event EventHandler ConnectionEstablished;
        /// <summary>
        /// Raises the ConnectionEstablished event
        /// </summary>
        internal virtual void OnConnectionEstablished()
        {
            ConnectionAvailable = true;
            ConnectionEstablished?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// The PacketReceived event occurs when a packet with an external ID was received
        /// </summary>
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;
        /// <summary>
        /// Raises the PacketReceived event and inverts the packet id.
        /// </summary>
        /// <param name="id">Packet ID</param>
        /// <param name="content">Packet content</param>
        internal virtual void OnPacketReceived(byte id, byte[] content)
        {
            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(Convert.ToByte(255 - id), content));
        }
        /// <summary>
        /// The ConnectionClosed event occurs when the connection was closed or VSL could not use it
        /// </summary>
        public event EventHandler<ConnectionClosedEventArgs> ConnectionClosed;
        /// <summary>
        /// Raises the ConnectionClosed event
        /// </summary>
        /// <param name="reason">Reason why the connection was closed</param>
        internal virtual void OnConnectionClosed(string reason)
        {
            if (ConnectionAvailable)
            {
                ConnectionAvailable = false;
                ConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs(reason));
            }
        }
        //  events>
        // <functions
        /// <summary>
        /// Connects the TCP Client asynchronously
        /// </summary>
        /// <param name="address">IP address or hostname</param>
        /// <param name="port">Port</param>
        /// <param name="serverKey">Public RSA key of the server</param>
        /// <returns></returns>
        public async Task ConnectAsync(string address, int port, string serverKey)
        {
            // <check args
            if (string.IsNullOrEmpty(address)) throw new ArgumentNullException();
            if (port < 0 || port > 65535) throw new ArgumentOutOfRangeException();
            if (string.IsNullOrEmpty(serverKey)) throw new ArgumentNullException();
            //  check args>

            TcpClient tcp = new TcpClient();
            await tcp.ConnectAsync(address, port);

            // <initialize component
            channel = new NetworkChannel(this, tcp);
            manager = new NetworkManager(this, serverKey);
            handler = new PacketHandler(this);
            //  initialize component>

            // <key exchange
            Task s = manager.SendPacketAsync(CryptographicAlgorithm.None, new P00Handshake(RequestType.DirectPublicKey));
            Task<byte[]> key = Task.Run(() => Crypt.AES.GenerateKey());
            Task<byte[]> civ = Task.Run(() => Crypt.AES.GenerateIV());
            Task<byte[]> siv = Task.Run(() => Crypt.AES.GenerateIV());
            manager.AesKey = await key;
            manager.SendIV = await civ;
            manager.ReceiveIV = await siv;
            await s;
            await manager.SendPacketAsync(CryptographicAlgorithm.RSA_2048, new P01KeyExchange(manager.AesKey, manager.SendIV,
                manager.ReceiveIV, Constants.VersionNumber, Constants.CompatibilityVersion, LatestProduct, OldestProduct));
            //  key exchange>
        }
        /// <summary>
        /// Sends a packet to the remotehost
        /// </summary>
        /// <param name="id">Packet ID</param>
        /// <param name="content">Packet data</param>
        /// <exception cref="ArgumentNullException"></exception>
        public async void SendPacket(byte id, byte[] content)
        {
            if (content == null) throw new ArgumentNullException("\"content\" must not be null");
            if (!ConnectionAvailable) throw new InvalidOperationException("You must not send a packet while there is no connection");
            await manager.SendPacketAsync(Convert.ToByte(255 - id), content);
        }
        /// <summary>
        /// Closes the TCP Connection, raises the related event and releases all associated resources. Instead of calling this method, ensure that the stream is properly disposed.
        /// </summary>
        public void CloseConnection(string reason)
        {
            OnConnectionClosed(reason);
            channel.CloseConnection();
            ExceptionHandler.StopTasks();
            Dispose();
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Specify whether managed objects should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    channel.Dispose();
                    ExceptionHandler.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~VSLSocket() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        //  functions>
    }
}