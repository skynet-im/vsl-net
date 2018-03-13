using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using VSL.FileTransfer;
using VSL.Net;
using VSL.Packet;

namespace VSL
{
    /// <summary>
    /// The client implementation of a VSL socket
    /// </summary>
    public sealed class VSLClient : VSLSocket
    {
        // <fields
        new internal PacketHandlerClient handler;
        private ushort latestProduct;
        private ushort oldestProduct;
        private int _networkBufferSize = Constants.ReceiveBufferSize;
        //  fields>

        // <constructor
#if !__IOS__
        /// <summary>
        /// Creates a VSL Client using <see cref="Threading.AsyncMode.ManagedThread"/> that has to be connected.
        /// </summary>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        public VSLClient(ushort latestProduct, ushort oldestProduct)
            : this(latestProduct, oldestProduct, ThreadManager.CreateManagedThread()) { }
#endif

        /// <summary>
        /// Creates a VSL Client that has to be connected.
        /// </summary>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="threadManager">Used to raise events and execute work items.</param>
        public VSLClient(ushort latestProduct, ushort oldestProduct, ThreadManager threadManager)
        {
            InitializeComponent(threadManager);

            this.latestProduct = latestProduct;
            this.oldestProduct = oldestProduct;
        }
        //  constructor>

        // <properties
        /// <summary>
        /// Gets or sets a value that specifies the size of the receive buffer of the Socket.
        /// </summary>
        public override int ReceiveBufferSize
        {
            get
            {
                if (channel != null)
                    return base.ReceiveBufferSize;
                else
                    return _networkBufferSize;
            }
            set
            {
                _networkBufferSize = value;
                if (channel != null)
                    base.ReceiveBufferSize = value;
            }
        }
        //  properties>

        // <functions
        /// <summary>
        /// Connects the TCP Client asynchronously.
        /// </summary>
        /// <param name="hostname">IP address or hostname.</param>
        /// <param name="port">TCP port to connect.</param>
        /// <param name="serverKey">Public RSA key of the server.</param>
        /// <returns></returns>
        public async Task ConnectAsync(string hostname, int port, string serverKey)
        {
            // <check args
            if (string.IsNullOrEmpty(hostname)) throw new ArgumentNullException();
            if (port < 0 || port > 65535) throw new ArgumentOutOfRangeException();
            if (string.IsNullOrEmpty(serverKey)) throw new ArgumentNullException();
            //  check args>

            IPAddress[] ipaddr = await Dns.GetHostAddressesAsync(hostname);
            TcpClient tcp = new TcpClient(AddressFamily.InterNetworkV6);
            tcp.Client.DualMode = true;
            await tcp.ConnectAsync(ipaddr, port);
            channel = new NetworkChannel(this, tcp.Client);

            // <initialize component
            manager = new NetworkManager(this, serverKey);
            handler = new PacketHandlerClient(this);
            base.handler = handler;
            StartInternal();
            channel.StartThreads();
            //  initialize component>

            // <key exchange
            Task s = Task.Run(() => manager.SendPacket(CryptoAlgorithm.None, new P00Handshake(RequestType.DirectPublicKey)));
            manager.GenerateKeys();
            await s;
            await Task.Run(() => manager.SendPacket(CryptoAlgorithm.RSA_2048_OAEP, new P01KeyExchange(manager.AesKey, manager.SendIV,
                manager.ReceiveIV, Constants.VersionNumber, Constants.CompatibilityVersion, latestProduct, oldestProduct)));
            //  key exchange>
        }
        //  functions>
    }
}