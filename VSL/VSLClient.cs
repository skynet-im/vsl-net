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
        /// <param name="latestProduct">The application version</param>
        /// <param name="oldestProduct">The oldest supported version</param>
        public VSLClient(ushort latestProduct, ushort oldestProduct)
        {
            InitializeComponent();

            ClientLatestProduct = latestProduct;
            ClientOldestProduct = oldestProduct;
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
        //  functions>
    }
}