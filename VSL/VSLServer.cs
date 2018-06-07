using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VSL.Network;

namespace VSL
{
    /// <summary>
    /// The server implementation of a VSL socket
    /// </summary>
    public sealed class VSLServer : VSLSocket
    {
        // <fields
        new internal PacketHandlerServer handler;
        internal string Keypair;
        internal ushort LatestProduct;
        internal ushort OldestProduct;
        //  fields>

        // <constructor
        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="socket">Connected <see cref="Socket"/>.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        public VSLServer(Socket socket, ushort latestProduct, ushort oldestProduct, string keypair)
        {
            InitializeComponent();

            LatestProduct = latestProduct;
            OldestProduct = oldestProduct;
            Keypair = keypair;
            channel = new NetworkChannel(socket, ExceptionHandler);
            manager = new NetworkManager(this, keypair);
            handler = new PacketHandlerServer(this);
            base.handler = handler;
        }
        //  constructor>

        // <functions
        /// <summary>
        /// Starts all threads for receiving and process packets.
        /// </summary>
        public void Start()
        {
            channel.StartThreads();
        }
        //  functions>
    }
}