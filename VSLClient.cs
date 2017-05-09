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
    /// The client implementation of a VSL socket
    /// </summary>
    public class VSLClient : VSLSocket
    {
        // <fields
        new internal NetworkChannelClient channel;
        new internal PacketHandlerClient handler;
        //  fields>
        // <constructor
        /// <summary>
        /// Creates a VSL Client that has to be connected
        /// </summary>
        /// <param name="appVersion">The application feature version (independent from VSL)</param>
        public VSLClient(uint appVersion)
        {
            InitializeComponent();

            TargetVersion = appVersion;
            channel = new NetworkChannelClient(this);
            base.channel = channel;
            handler = new PacketHandlerClient(this);
            base.handler = handler;
        }
        // constructor>
        // <functions
        /// <summary>
        /// Connects the TCP Client asynchronously
        /// </summary>
        /// <param name="address">IP address or hostname</param>
        /// <param name="port">Port</param>
        /// <param name="serverKey">Public RSA key of the server</param>
        /// <returns></returns>
        public Task ConnectAsync(string address, int port, string serverKey)
        {
            // <check args
            if (string.IsNullOrEmpty(address)) throw new ArgumentNullException();
            if (port < 0 || port > 65535) throw new ArgumentOutOfRangeException();
            if (string.IsNullOrEmpty(serverKey)) throw new ArgumentNullException();
            // check args>
            return channel.Connect(address, port, serverKey);
        }

        /// <summary>
        /// Connects the TCP Client asynchronously
        /// </summary>
        /// <param name="address">IP address or hostname</param>
        /// <param name="port">Port</param>
        /// <param name="serverKey">Public RSA key of the server</param>
        /// <returns></returns>
        [Obsolete("VSLClient.Connect(string, int, string) is deprecated, please use VSLClient.ConnectAsync(string, int, string) instead.", false)]
        public Task Connect(string address, int port, string serverKey)
        {
            // <check args
            if (string.IsNullOrEmpty(address)) throw new ArgumentNullException();
            if (port < 0 || port > 65535) throw new ArgumentOutOfRangeException();
            if (string.IsNullOrEmpty(serverKey)) throw new ArgumentNullException();
            // check args>
            return channel.Connect(address, port, serverKey);
        }
        //  functions>
    }
}