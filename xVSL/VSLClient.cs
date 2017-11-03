using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using VSL.FileTransfer;
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
        /// <summary>
        /// Access file transfer functions.
        /// </summary>
        new public FileTransferClient FileTransfer { get; internal set; }
        internal ushort LatestProduct;
        internal ushort OldestProduct;
        private int _networkBufferSize = Constants.ReceiveBufferSize;
        //  fields>

        // <constructor
        /// <summary>
        /// Creates a VSL Client that has to be connected.
        /// </summary>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        public VSLClient(ushort latestProduct, ushort oldestProduct)
            : this(latestProduct, oldestProduct, ThreadMgr.InvokeMode.Dispatcher) { }

        /// <summary>
        /// Creates a VSL Client that has to be connected.
        /// </summary>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="mode">The way how events are invoked.</param>
        public VSLClient(ushort latestProduct, ushort oldestProduct, ThreadMgr.InvokeMode mode)
        {
            InitializeComponent(mode);

            LatestProduct = latestProduct;
            OldestProduct = oldestProduct;

            FileTransfer = new FileTransferClient(this);
            base.FileTransfer = FileTransfer;
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

            IPAddress[] ipaddr = await Dns.GetHostAddressesAsync(address);
            TcpClient tcp = new TcpClient(AddressFamily.InterNetworkV6);
            tcp.Client.DualMode = true;
            await tcp.ConnectAsync(ipaddr, port);
            channel = new NetworkChannel(this, tcp.Client);

            // <initialize component
            manager = new NetworkManager(this, serverKey);
            handler = new PacketHandlerClient(this);
            base.handler = handler;
            channel.StartThreads();
            //  initialize component>

            // <key exchange
            Task s = Task.Run(() => manager.SendPacket(CryptographicAlgorithm.None, new P00Handshake(RequestType.DirectPublicKey)));
            manager.GenerateKeys();
            await s;
            await Task.Run(() => manager.SendPacket(CryptographicAlgorithm.RSA_2048_OAEP, new P01KeyExchange(manager.AesKey, manager.SendIV,
                manager.ReceiveIV, Constants.VersionNumber, Constants.CompatibilityVersion, LatestProduct, OldestProduct)));
            //  key exchange>
        }
        //  functions>
    }
}