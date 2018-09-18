using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using VSL.FileTransfer;
using VSL.Network;

namespace VSL
{
    /// <summary>
    /// The base class for VSL implementations
    /// </summary>
    public abstract class VSLSocket : IDisposable
    {
        // fields
        private readonly object connectionLostLock;
        private bool connectionEstablished;
        private bool connectionLost;

        // components
        /// <summary>
        /// Gets or sets the settings for this socket.
        /// </summary>
        public SocketSettings Settings { get; }

        /// <summary>
        /// Access file transfer functions.
        /// </summary>
        public FTSocket FileTransfer { get; private set; }

        internal NetworkChannel Channel { get; set; }
        internal NetworkManager Manager { get; set; }
        internal PacketHandler Handler { get; set; }
        internal InvokationManager ThreadManager { get; private set; }
        internal ExceptionHandler ExceptionHandler { get; private set; }

        /// <summary>
        /// Initializes all non-child-specific components.
        /// </summary>
        protected VSLSocket(SocketSettings settings)
        {
            Settings = settings;
            ThreadManager = new InvokationManager();
            ExceptionHandler = new ExceptionHandler(this);
            FileTransfer = new FTSocket(this);
            connectionLostLock = new object();
        }

        internal static MemoryCache<SocketAsyncEventArgs> CreateFakeCache()
        {
            return new MemoryCache<SocketAsyncEventArgs>(0, () =>
            {
                int length = 65536;
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.SetBuffer(new byte[length], 0, length);
                return e;
            });
        }

        #region properties
        /// <summary>
        /// Gets a value indicating whether a working and secure connection is available.
        /// </summary>
        public bool ConnectionAvailable { get; private set; }
        /// <summary>
        /// Gets the protocol version that is used for this connection as <see cref="string"/>.
        /// </summary>
        public string ConnectionVersionString => VersionManager.GetVersion(ConnectionVersion);
        /// <summary>
        /// Gets the protocol version that is used for this connection.
        /// </summary>
        public ushort? ConnectionVersion { get; internal set; }
        /// <summary>
        /// Gets the total count of bytes, received in this session until now.
        /// </summary>
        public long ReceivedBytes => Channel.ReceivedBytes;
        /// <summary>
        /// Gets the total count of bytes, sent in this session until now.
        /// </summary>
        public long SentBytes => Channel.SentBytes;
        #endregion
        #region events
        /// <summary>
        /// The ConnectionEstablished event occurs when the connection was build up and the key exchange was finished
        /// </summary>
        public event EventHandler ConnectionEstablished;
        /// <summary>
        /// Raises the ConnectionEstablished event
        /// </summary>
        internal virtual void OnConnectionEstablished()
        {
            connectionEstablished = true;
            ConnectionAvailable = true;
            ThreadManager.Post(() => ConnectionEstablished?.Invoke(this, new EventArgs()));
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
            PacketReceivedEventArgs args = new PacketReceivedEventArgs(Convert.ToByte(255 - id), content);
            ThreadManager.Post(() => PacketReceived?.Invoke(this, args));
        }
        /// <summary>
        /// The ConnectionClosed event occurs when the connection was closed or VSL could not use it.
        /// </summary>
        public event EventHandler<ConnectionClosedEventArgs> ConnectionClosed;
        /// <summary>
        /// Raises the ConnectionClosed event.
        /// </summary>
        /// <param name="e"></param>
        internal virtual void OnConnectionClosed(ConnectionClosedEventArgs e)
        {
            ThreadManager.Post(() => ConnectionClosed?.Invoke(this, e));
        }
        #endregion
        #region logging
#if DEBUG
        public Action<VSLSocket, string> LogHandler { get; set; }
        internal void Log(string message)
        {
            LogHandler?.Invoke(this, message);
        }
#endif
        #endregion
        // <functions
        #region Receive
        /// <summary>
        /// Starts to receive messsages from the connected remote host.
        /// </summary>
        protected async void StartReceiveLoop()
        {
            while (!connectionLost)
            {
                if (!await Manager.ReceivePacketAsync())
                    break;
            }
        }
        #endregion
        #region Send
        /// <summary>
        /// Sends a packet to the remotehost asynchronously.
        /// </summary>
        /// <param name="id">Packet ID</param>
        /// <param name="content">Packet data</param>
        /// <returns>Returns true when sending succeeded and false when a network error occured.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="ObjectDisposedException"/>
        public Task<bool> SendPacketAsync(byte id, byte[] content)
        {
            if (id > byte.MaxValue - Constants.InternalPacketCount)
                throw new ArgumentOutOfRangeException(nameof(id), id, $"ID must be lower or equal than {byte.MaxValue - Constants.InternalPacketCount}.");
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            if (disposedValue)
                throw new ObjectDisposedException(GetType().FullName);
            if (!connectionEstablished)
                throw new InvalidOperationException("You have to wait until a secure connection is established before you send a packet.");

            return Manager.SendPacketAsync((byte)(255 - id), content);
        }
        #endregion
        #region Close
        /// <summary>
        /// Closes the TCP Connection, raises the related event and releases all associated resources.
        /// </summary>
        /// <param name="message">The reason to print and share in the related event.</param>
        /// <param name="ex">The exception that caused disconnect.</param>
        /// <exception cref="ObjectDisposedException"/>
        public void CloseConnection(string message, Exception ex)
        {
            if (disposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            lock (connectionLostLock)
                if (!connectionLost) // To detect redundant calls
                {
                    ClosePrivate(ConnectionCloseReason.UserRequested, message, ex);
                    Dispose();
                }
        }

        /// <summary>
        /// Closes the TCP Connection and raises the related event.
        /// </summary>
        internal void CloseInternal(ConnectionCloseReason reason, string message, Exception ex)
        {
            lock (connectionLostLock)
                if (!connectionLost) // To detect redundant calls
                {
                    ClosePrivate(reason, message, ex);
                }
        }

        /// <summary>
        /// Sets all variables to the closed state.
        /// </summary>
        private void ClosePrivate(ConnectionCloseReason reason, string message, Exception ex)
        {
            ConnectionAvailable = false;
            connectionLost = true;
            Channel.Shutdown();
            // TODO: Cancel running file transfer
            OnConnectionClosed(new ConnectionClosedEventArgs(reason, message, ex));
        }
        #endregion
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
                    // -TODO: dispose managed state (managed objects).
                    Channel?.Dispose();
                    Manager?.Dispose();
                }

                // -TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // -TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // -TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
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
            // -TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        //  functions>
    }
}