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
    public class VSLServer : VSLSocket
    {
        // <fields
        new internal NetworkManagerServer manager;
        new internal PacketHandlerServer handler;
        /// <summary>
        /// Access file transfer functions.
        /// </summary>
        new public FileTransferServer FileTransfer;
        internal string Keypair;
        internal ushort LatestProduct;
        internal ushort OldestProduct;
        //  fields>
        // <constructor
        /// <summary>
        /// Creates a VSL server for the specified client.
        /// </summary>
        /// <param name="tcp">Connected TcpClient.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        public VSLServer(TcpClient tcp, ushort latestProduct, ushort oldestProduct, string keypair) : this(tcp, latestProduct, oldestProduct, keypair, ThreadMgr.InvokeMode.ManagedThread)
        {
        }
        // <constructor
        /// <summary>
        /// Creates a VSL server for the specified client.
        /// </summary>
        /// <param name="tcp">Connected TcpClient.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        /// <param name="mode">The way how events are invoked.</param>
        public VSLServer(TcpClient tcp, ushort latestProduct, ushort oldestProduct, string keypair, ThreadMgr.InvokeMode mode)
        {
            InitializeComponent(mode);

            LatestProduct = latestProduct;
            OldestProduct = oldestProduct;
            Keypair = keypair;
            channel = new NetworkChannel(this, tcp);
            manager = new NetworkManagerServer(this, keypair);
            base.manager = manager;
            handler = new PacketHandlerServer(this);
            base.handler = handler;
            FileTransfer = new FileTransferServer(this);
            base.FileTransfer = FileTransfer;
            channel.StartThreads();
        }
        //  constructor>
        // <functions
        //  functions>
    }
}