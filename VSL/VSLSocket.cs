using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace VSL
{
    /// <summary>
    /// The base class for VSL implementations
    /// </summary>
    public abstract class VSLSocket
    {
        // <fields
        internal bool ConnectionAvailable = false;
        internal ushort ClientLatestProduct;
        internal ushort ClientOldestProduct;
        internal NetworkChannel channel;
        internal NetworkManager manager;
        internal PacketHandler handler;
        //  fields>
        // <constructor
        /// <summary>
        /// Initializes all non-child-specific components
        /// </summary>
        internal void InitializeComponent()
        {
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
            ConnectionEstablished?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// The PacketReceived event occurs when a packet with an external ID was received
        /// </summary>
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;
        /// <summary>
        /// Raises the PacketReceived event
        /// </summary>
        /// <param name="id">Packet ID</param>
        /// <param name="content">Packet content</param>
        internal virtual void OnPacketReceived(byte id, byte[] content)
        {
            if (!handler.TryHandlePacket(id, content))
                PacketReceived?.Invoke(this, new PacketReceivedEventArgs(id, content));
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
            ConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs(reason));
        }
        //  events>
        // <functions
        /// <summary>
        /// Sends a packet to the remotehost
        /// </summary>
        /// <param name="id">Packet ID</param>
        /// <param name="content">Packet data</param>
        public void SendPacket(byte id, byte[] content)
        {
            if (content == null)
                throw new ArgumentNullException("\"content\" must not be null");
            channel.SendPacket(id, content);
        }
        /// <summary>
        /// Sends a packet to the remotehost
        /// </summary>
        /// <param name="packet"></param>
        internal void SendPacket(IPacket packet)
        {
            SendPacket(packet.ID, packet.WritePacket());
        }
        /// <summary>
        /// Closes the TCP Connection
        /// </summary>
        public void CloseConnection()
        {
            channel.CloseConnection();
        }
        //  functions>
    }
}