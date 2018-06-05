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
        /// Creates a VSL server for the specified client using <see cref="Threading.AsyncMode.ThreadPool"/>. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="socket">Connected <see cref="Socket"/>.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        public VSLServer(Socket socket, ushort latestProduct, ushort oldestProduct, string keypair)
            : this(socket, latestProduct, oldestProduct, keypair, ThreadManager.CreateThreadPool()) { }

        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="socket">Connected <see cref="Socket"/>.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        /// <param name="threadManager">Used to invoke events.</param>
        public VSLServer(Socket socket, ushort latestProduct, ushort oldestProduct, string keypair, ThreadManager threadManager)
        {
            InitializeComponent(threadManager);

            LatestProduct = latestProduct;
            OldestProduct = oldestProduct;
            Keypair = keypair;
            channel = new NetworkChannel(this, socket);
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
            StartInternal();
            channel.StartThreads();
        }
        //  functions>
    }
}