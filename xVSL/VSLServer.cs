using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
#if WINDOWS_UWP
using Windows.Networking.Sockets;
#else
using System.Net.Sockets;
#endif
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
#if WINDOWS_UWP
        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="socket">Connected <see cref="StreamSocket"/>.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        public VSLServer(StreamSocket socket, ushort latestProduct, ushort oldestProduct, string keypair)
            : this(socket, latestProduct, oldestProduct, keypair, ThreadMgr.InvokeMode.ManagedThread) { }
#else
        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="socket">Connected <see cref="Socket"/>.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        public VSLServer(Socket socket, ushort latestProduct, ushort oldestProduct, string keypair)
            : this(socket, latestProduct, oldestProduct, keypair, ThreadMgr.InvokeMode.ManagedThread) { }
#endif
#if WINDOWS_UWP
        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="socket">Connected <see cref="StreamSocket"/>.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        /// <param name="mode">The way how events are invoked.</param>
        public VSLServer(StreamSocket socket, ushort latestProduct, ushort oldestProduct, string keypair, ThreadMgr.InvokeMode mode)
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
#else
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
#endif
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