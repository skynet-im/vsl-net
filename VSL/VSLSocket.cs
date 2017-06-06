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
        internal bool ConnectionAvailable = false;
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
        //  fields>
        // <constructor
        /// <summary>
        /// Initializes all non-child-specific components.
        /// </summary>
        internal void InitializeComponent()
        {
            ExceptionHandler = new ExceptionHandler(this);
            Logger = new Logger(this);
        }
        //  constructor>
        // <properties
        /// <summary>
        /// Gets or sets the size of the second receive buffer
        /// </summary>
        public int NetworkBufferSize
        {
            get
            {
                return channel.NetworkBufferSize;
            }
            set
            {
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