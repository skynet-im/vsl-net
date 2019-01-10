using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VSL.Crypt;
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
        private readonly IVSLCallback callback;

        // components
        /// <summary>
        /// Gets or sets the settings for this socket.
        /// </summary>
        public SocketSettings Settings { get; }

        /// <summary>
        /// Access file transfer functions.
        /// </summary>
        public FTSocket FileTransfer { get; }

        internal NetworkChannel Channel { get; set; }
        internal NetworkManager Manager { get; set; }
        internal PacketHandler Handler { get; set; }
        internal InvokationManager ThreadManager { get; }
        internal ExceptionHandler ExceptionHandler { get; }

        /// <summary>
        /// Initializes all non-child-specific components.
        /// </summary>
        protected VSLSocket(SocketSettings settings, IVSLCallback callback)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            settings.RsaKey.AssertValid();
            ThreadManager = new InvokationManager();
            ExceptionHandler = new ExceptionHandler(this);
            FileTransfer = new FTSocket(this);
            connectionLostLock = new object();
            callback.OnInstanceCreated(this);
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
        /// Occurs once a secure connection has been established.
        /// </summary>
        internal virtual Task OnConnectionEstablished()
        {
            connectionEstablished = true;
            ConnectionAvailable = true;
            return callback.OnConnectionEstablished();
        }
        /// <summary>
        /// Occurs when a packet with an external ID was received.
        /// </summary>
        /// <param name="id">Native packet ID</param>
        /// <param name="content">Packet content</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual Task OnPacketReceived(byte id, byte[] content)
        {
            return callback.OnPacketReceived((byte)(255 - id), content);
        }
        /// <summary>
        /// Occurs when the connection was closed or VSL could not use it.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OnConnectionClosed(ConnectionCloseReason reason, string message, Exception exception)
        {
            callback.OnConnectionClosed(reason, message, exception);
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
        public bool CloseConnection(string message, Exception ex = null)
        {
            lock (connectionLostLock)
                if (!connectionLost) // To detect redundant calls
                {
                    ClosePrivate(ConnectionCloseReason.UserRequested, message, ex);
                    Dispose(true);
                    return true;
                }
                else return false;
        }

        /// <summary>
        /// Closes the TCP Connection and raises the related event.
        /// </summary>
        internal bool CloseInternal(ConnectionCloseReason reason, string message, Exception exception)
        {
            lock (connectionLostLock)
                if (!connectionLost) // To detect redundant calls
                {
                    ClosePrivate(reason, message, exception);
                    return true;
                }
                else return false;
        }

        /// <summary>
        /// Sets all variables to the closed state.
        /// </summary>
        private void ClosePrivate(ConnectionCloseReason reason, string message, Exception exception)
        {
            ConnectionAvailable = false;
            connectionLost = true;
            Channel.Shutdown();
            FileTransfer?.Dispose(); // Cancel running file transfer
            OnConnectionClosed(reason, message, exception);
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
                    Channel?.Dispose();
                    Manager?.Dispose();
                }

                disposedValue = true;
            }
        }

        // ~VSLSocket() {
        //   Dispose(false);
        // }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }
        #endregion
        //  functions>
    }
}