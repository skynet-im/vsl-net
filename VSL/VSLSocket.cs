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
    public abstract class VSLSocket
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
        /// Stops the network channel, closes the TCP Connection and raises the related event
        /// </summary>
        public void CloseConnection(string reason)
        {
            OnConnectionClosed(reason);
            channel.CloseConnection(reason);
            ExceptionHandler.StopTasks();
        }
        //  functions>
    }
}