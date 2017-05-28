using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Creates a VSL listener for the specified client
        /// </summary>
        /// <param name="tcp">TCP Listener</param>
        /// <param name="latestProduct">The application version</param>
        /// <param name="oldestProduct">The oldest supported version</param>
        /// <param name="keypair">The RSA-keypair of the server application</param>
        public VSLServer(TcpClient tcp, ushort latestProduct, ushort oldestProduct, string keypair)
        {
            InitializeComponent();

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
        }
        //  constructor>
        // <functions
        //  functions>
    }
}