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
        /// <summary>
        /// Access file transfer functions.
        /// </summary>
        new public FileTransferServer FileTransfer { get; internal set; }
        internal string Keypair;
        internal ushort LatestProduct;
        internal ushort OldestProduct;
        //  fields>

        // <constructor
        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="tcp">Connected TcpClient.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        [Obsolete("VSLServer.VSLServer(TcpClient tcp, ...) is deprecated, please use VSLServer.VSLServer(Socket socket, ...) instead.", true)]
        // TODO: Remove in v1.1.18.0
        public VSLServer(TcpClient tcp, ushort latestProduct, ushort oldestProduct, string keypair)
            : this(tcp, latestProduct, oldestProduct, keypair, ThreadMgr.InvokeMode.ManagedThread) { }

        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="tcp">Connected TcpClient.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        /// <param name="mode">The way how events are invoked.</param>
        [Obsolete("VSLServer.VSLServer(TcpClient tcp, ...) is deprecated, please use VSLServer.VSLServer(Socket socket, ...) instead.", true)]
        // TODO: Remove in v1.1.18.0
        public VSLServer(TcpClient tcp, ushort latestProduct, ushort oldestProduct, string keypair, ThreadMgr.InvokeMode mode)
            : this(tcp.Client, latestProduct, oldestProduct, keypair, mode) { }

        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="socket">Connected <see cref="Socket"/>.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        public VSLServer(Socket socket, ushort latestProduct, ushort oldestProduct, string keypair)
            : this(socket, latestProduct, oldestProduct, keypair, ThreadMgr.InvokeMode.ManagedThread) { }

        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="socket">Connected <see cref="Socket"/>.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        /// <param name="mode">The way how events are invoked.</param>
        public VSLServer(Socket socket, ushort latestProduct, ushort oldestProduct, string keypair, ThreadMgr.InvokeMode mode)
        {
            InitializeComponent(mode);

            LatestProduct = latestProduct;
            OldestProduct = oldestProduct;
            Keypair = keypair;
            channel = new NetworkChannel(this, socket);
            manager = new NetworkManager(this, keypair);
            handler = new PacketHandlerServer(this);
            base.handler = handler;
            FileTransfer = new FileTransferServer(this);
            base.FileTransfer = FileTransfer;
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