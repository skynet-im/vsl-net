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
    public class VSLListener : VSLSocket
    {
        // <fields
        new internal NetworkChannelServer channel;
        new internal PacketHandlerServer handler;
        internal string Keypair;
        //  fields>

        // <constructor
        /// <summary>
        /// Creates a VSL listener for the specified client
        /// </summary>
        /// <param name="tcp">TCP Listener</param>
        /// <param name="appVersion">The application feature version (independent from VSL)</param>
        /// <param name="keypair">The RSA-keypair of the server application</param>
        public VSLListener(TcpClient tcp, uint appVersion, string keypair)
        {
            InitializeComponent();

            TargetVersion = appVersion;
            Keypair = keypair;
            channel = new NetworkChannelServer(this);
            base.channel = channel;
            channel.Connect(tcp);
            handler = new PacketHandlerServer(this);
            base.handler = handler;
        }
        //  constructor>

        // <functions

        //  functions>
    }
}