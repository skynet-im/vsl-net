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
        new internal NetworkManagerClient manager;
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
            channel = new NetworkChannel(this);
            manager = new NetworkManagerClient(this);
            base.manager = manager;
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
        public async Task ConnectAsync(string address, int port, string serverKey)
        {
            // <check args
            if (string.IsNullOrEmpty(address)) throw new ArgumentNullException();
            if (port < 0 || port > 65535) throw new ArgumentOutOfRangeException();
            if (string.IsNullOrEmpty(serverKey)) throw new ArgumentNullException();
            //  check args>
            // <resolve hostname
            IPAddress[] ips;
            try
            {
                ips = await Dns.GetHostAddressesAsync(address);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not resolve hostname " + address + ": " + ex);
            }
            // resolve hostname>

            // <connect
            TcpClient tcp = new TcpClient();
            bool couldConnect = false;
            foreach (IPAddress ip in ips)
            {
                try
                {
                    await tcp.ConnectAsync(ip, port);
                    channel.Connect(tcp);
                    couldConnect = true;
                    Console.WriteLine(ip.ToString());
                    break;
                }
                catch { }
            }
            if (!couldConnect) throw new Exception("Could not connect to the specified host");
            // connect>

            // <key exchange
            // TODO: Implement Key Exchange
            //  key exchange>
        }
        //  functions>
    }
}