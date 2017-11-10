using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using VSL.FileTransfer;

namespace VSL
{
    /// <summary>
    /// The base class for VSL implementations
    /// </summary>
    public abstract class VSLSocket : IDisposable
    {
        // <fields
        /// <summary>
        /// Specifies if a working and secure connection is available.
        /// </summary>
        private bool connectionAvailable = false;
        private object connectionLostLock;
        private bool connectionLost = false;
        private DateTime connectionLostTime = DateTime.MinValue;
        private DateTime disposingTime = DateTime.MinValue;
        internal NetworkChannel channel;
        internal NetworkManager manager;
        internal PacketHandler handler;
        /// <summary>
        /// Gets the manager for event invocation and load balancing.
        /// </summary>
        public ThreadManager ThreadManager { get; internal set; }
        /// <summary>
        /// Access file transfer functions.
        /// </summary>
        public FileTransferSocket FileTransfer { get; internal set; }
        internal ExceptionHandler ExceptionHandler;
        /// <summary>
        /// Configure necessary console output.
        /// </summary>
        public Logger Logger { get; internal set; }
        //  fields>
        // <constructor
        /// <summary>
        /// Initializes all non-child-specific components.
        /// </summary>
        protected void InitializeComponent(ThreadManager threadManager)
        {
            ThreadManager = threadManager;
            threadManager.Assign(this);
            ExceptionHandler = new ExceptionHandler(this);
            Logger = new Logger(this);
            connectionLostLock = new object();
        }
        /// <summary>
        /// Starts the assigned ThreadManager.
        /// </summary>
        protected void StartInternal()
        {
            ThreadManager.Start();
        }
        //  constructor>
        #region properties
        /// <summary>
        /// Gets a value indicating whether a working and secure connection is available.
        /// </summary>
        public bool ConnectionAvailable => connectionAvailable;
        /// <summary>
        /// Gets the protocol version that is used for this connection as <see cref="string"/>.
        /// </summary>
        public string ConnectionVersionString { get; private set; }
        private ushort _connectionVersion;
        /// <summary>
        /// Gets the protocol version that is used for this connection.
        /// </summary>
        public ushort ConnectionVersion
        {
            get => _connectionVersion;
            internal set
            {
                _connectionVersion = value;
                ConnectionVersionString = VersionManager.GetVersion(value);
            }
        }
        /// <summary>
        /// Gets or sets a value that specifies the size of the receive buffer of the Socket.
        /// </summary>
        public virtual int ReceiveBufferSize
        {
            get
            {
                return channel.ReceiveBufferSize;
            }
            set
            {
                if (!disposedValue)
                    channel.ReceiveBufferSize = value;
            }
        }
        /// <summary>
        /// Gets or sets the sleep time background threads while waiting for work.
        /// </summary>
        public int SleepTime { get; set; } = Constants.SleepTime;
        /// <summary>
        /// Gets the total count of bytes, received in this session until now.
        /// </summary>
        public long ReceivedBytes => channel.ReceivedBytes;
        /// <summary>
        /// Gets the total count of bytes, sent in this session until now.
        /// </summary>
        public long SentBytes => channel.SentBytes;
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
            connectionAvailable = true;
            ThreadManager.Start();
            ThreadManager.QueueWorkItem((ct) => ConnectionEstablished?.Invoke(this, new EventArgs()));
            if (Logger.InitI)
                Logger.I("New connection established using VSL " + ConnectionVersionString);
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
            ThreadManager.QueueWorkItem((ct) => PacketReceived?.Invoke(this, args));
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
            ThreadManager.QueueWorkItem((ct) => ConnectionClosed?.Invoke(this, e));
        }
        #endregion
        // <functions
        #region Send
        /// <summary>
        /// Sends a packet to the remotehost.
        /// </summary>
        /// <param name="id">Packet ID</param>
        /// <param name="content">Packet data</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidOperationException"/>
        public bool SendPacket(byte id, byte[] content)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (id >= 246) throw new ArgumentOutOfRangeException("id", "must be lower than 246 because of internal VSL packets");
            if (disposedValue && (DateTime.Now - disposingTime).TotalMilliseconds > 100)
                throw new ObjectDisposedException("VSL.VSLSocket", "This VSLSocket was disposed over 100ms ago.");
            if (!ConnectionAvailable)
            {
                if (!connectionLost)
                    throw new InvalidOperationException("You have to wait until a secure connection is established before you send a packet.");
                else
                {
                    double spanMilliseconds = (DateTime.Now - connectionLostTime).TotalMilliseconds;
                    string spanText;
                    if (spanMilliseconds < 100)
                        return false;
                    if (spanMilliseconds < 10000)
                        spanText = Math.Round(spanMilliseconds).ToString() + " ms";
                    else
                        spanText = Math.Round(spanMilliseconds, 1).ToString() + " sek";
                    throw new InvalidOperationException(string.Format("VSL has lost its connection {0} ago. Build up a new connection before sending a packet.", spanText));
                }
            }
            return manager.SendPacket(Convert.ToByte(255 - id), content);
        }
        /// <summary>
        /// Sends a packet to the remotehost asynchronously.
        /// </summary>
        /// <param name="id">Packet ID</param>
        /// <param name="content">Packet data</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="ObjectDisposedException"/>
        public async Task<bool> SendPacketAsync(byte id, byte[] content)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (id >= 246) throw new ArgumentOutOfRangeException("id", "must be lower than 246 because of internal VSL packets");
            if (disposedValue && (DateTime.Now - disposingTime).TotalMilliseconds > 100)
                throw new ObjectDisposedException("VSL.VSLSocket", "This VSLSocket was disposed over 100ms ago.");
            if (!ConnectionAvailable)
            {
                if (!connectionLost)
                    throw new InvalidOperationException("You have to wait until a secure connection is established before you send a packet.");
                else
                {
                    double spanMilliseconds = (DateTime.Now - connectionLostTime).TotalMilliseconds;
                    string spanText;
                    if (spanMilliseconds < 100)
                        return false;
                    if (spanMilliseconds < 10000)
                        spanText = Math.Round(spanMilliseconds).ToString() + " ms";
                    else
                        spanText = Math.Round(spanMilliseconds, 1).ToString() + " sek";
                    throw new InvalidOperationException(string.Format("VSL has lost its connection {0} ago. Build up a new connection before sending a packet.", spanText));
                }
            }
            return await Task.Run(() => manager.SendPacket(Convert.ToByte(255 - id), content));
        }
        #endregion
        #region Close
        /// <summary>
        /// Closes the TCP Connection, raises the related event and releases all associated resources.
        /// </summary>
        /// <param name="reason">The reason to print and share in the related event.</param>
        /// <exception cref="ObjectDisposedException"/>
        public void CloseConnection(string reason)
        {
            if (disposedValue && (DateTime.Now - disposingTime).TotalMilliseconds > 100)
                throw new ObjectDisposedException("VSL.VSLSocket", "This VSLSocket was disposed over 100ms ago.");
            lock (connectionLostLock)
                if (!connectionLost) // To detect redundant calls
                {
                    ConnectionClosedEventArgs e = PrepareOnConnectionClosed(reason);
                    if (Logger.InitI)
                        Logger.I("Connection was forcibly closed: " + reason);
                    channel.CloseConnection();
                    OnConnectionClosed(e);
                    ThreadManager.Close();
                    Dispose();
                }
        }

        /// <summary>
        /// Closes the TCP Connection and raises the related event.
        /// </summary>
        /// <param name="exception">The exception text to share in the related event.</param>
        internal void CloseInternal(string exception)
        {
            lock (connectionLostLock)
                if (!connectionLost) // To detect redundant calls
                {
                    ConnectionClosedEventArgs e = PrepareOnConnectionClosed(exception);
                    channel.CloseConnection();
                    OnConnectionClosed(e);
                }
        }

        /// <summary>
        /// Sets all variables to the closed state.
        /// </summary>
        /// <returns>Returns the <see cref="ConnectionClosedEventArgs"/> for the upcoming event.</returns>
        private ConnectionClosedEventArgs PrepareOnConnectionClosed(string reason)
        {
            connectionAvailable = false;
            connectionLost = true;
            connectionLostTime = DateTime.Now;
            ThreadManager.Shutdown();
            return new ConnectionClosedEventArgs(reason, channel.ReceivedBytes, channel.SentBytes);
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
                    channel?.Dispose();
                    manager?.Dispose();
                    ThreadManager?.Dispose();
                }

                // -TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // -TODO: set large fields to null.

                disposingTime = DateTime.Now;
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