using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
#if WINDOWS_UWP
using Windows.Networking.Sockets;
#else
using System.Net.Sockets;
#endif
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

#if WINDOWS_UWP
            StreamSocket socket = new StreamSocket();
            await socket.ConnectAsync(new Windows.Networking.EndpointPair(new Windows.Networking.HostName(""), "",
                new Windows.Networking.HostName(address), port.ToString()));
            channel = new NetworkChannel(this, socket);
#else
            TcpClient tcp = new TcpClient();
            await tcp.ConnectAsync(address, port);
            channel = new NetworkChannel(this, tcp.Client);
#endif

            // <initialize component
            manager = new NetworkManager(this, serverKey);
            handler = new PacketHandlerClient(this);
            base.handler = handler;
            channel.StartThreads();
            //  initialize component>

            // <key exchange
            Task s = manager.SendPacketAsync(CryptographicAlgorithm.None, new P00Handshake(RequestType.DirectPublicKey));
            Random random = new Random();
            byte[] key = new byte[32];
            random.NextBytes(key);
            manager.AesKey = key;
            byte[] siv = new byte[16];
            random.NextBytes(siv);
            manager.SendIV = siv;
            byte[] riv = new byte[16];
            random.NextBytes(riv);
            manager.ReceiveIV = riv;
            manager.Ready4Aes = true;
            await s;
            await manager.SendPacketAsync(CryptographicAlgorithm.RSA_2048, new P01KeyExchange(manager.AesKey, manager.SendIV,
                manager.ReceiveIV, Constants.VersionNumber, Constants.CompatibilityVersion, LatestProduct, OldestProduct));
            //  key exchange>
        }
        //  functions>
    }
}