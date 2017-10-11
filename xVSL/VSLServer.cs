using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using VSL.FileTransfer;
using VSL.Server;

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
        internal SharedServerHelper Helper;
        //  fields>

        // <constructor
        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="socket">Connected <see cref="Socket"/>.</param>
        /// <param name="helper">Shared instance of ServerHelper.</param>
        public VSLServer(Socket socket, SharedServerHelper helper)
        {
            InitializeComponent(helper.InvokeMode);

            Helper = helper;
            LatestProduct = helper.LatestProduct;
            OldestProduct = helper.OldestProduct;
            Keypair = helper.Keypair;
            channel = new NetworkChannel(this, socket);
            manager = new NetworkManager(this, Keypair);
            handler = new PacketHandlerServer(this);
            base.handler = handler;
            FileTransfer = new FileTransferServer(this);
            base.FileTransfer = FileTransfer;
        }

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