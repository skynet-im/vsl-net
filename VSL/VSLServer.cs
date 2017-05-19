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
    /// The server implementation of a VSL socket
    /// </summary>
    public class VSLServer : VSLSocket
    {
        // <fields
        new internal NetworkManagerServer manager;
        new internal PacketHandlerServer handler;
        internal string Keypair;
        internal ushort ServerLatestProduct;
        internal ushort ServerOldestProduct;
        internal ushort ClientLatestVSL;
        internal ushort ClientOldestVSL;
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

            ServerLatestProduct = latestProduct;
            ServerOldestProduct = oldestProduct;
            Keypair = keypair;
            channel = new NetworkChannel(this, tcp);
            channel.Connect(tcp);
            manager = new NetworkManagerServer(this);
            base.manager = manager;
            handler = new PacketHandlerServer(this);
            base.handler = handler;
        }
        //  constructor>
        // <functions
        //  functions>
    }
}