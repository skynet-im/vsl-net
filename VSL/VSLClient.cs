using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using VSL.Net;
using VSL.Packet;

namespace VSL
{
    /// <summary>
    /// The client implementation of a VSL socket
    /// </summary>
    public sealed class VSLClient : VSLSocket
    {
        new internal PacketHandlerClient handler;
        private readonly ushort latestProduct;
        private readonly ushort oldestProduct;
        private int _networkBufferSize = Constants.ReceiveBufferSize;
        private TaskCompletionSource<int> tcs;

        /// <summary>
        /// Creates a VSL Client that has to be connected.
        /// </summary>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        public VSLClient(ushort latestProduct, ushort oldestProduct)
        {
            InitializeComponent();

            this.latestProduct = latestProduct;
            this.oldestProduct = oldestProduct;
        }

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

        /// <summary>
        /// Connects the TCP Client asynchronously.
        /// </summary>
        /// <param name="hostname">IP address or hostname.</param>
        /// <param name="port">TCP port to connect.</param>
        /// <param name="serverKey">Public RSA key of the server.</param>
        /// <returns></returns>
        public Task ConnectAsync(string hostname, int port, string serverKey)
        {
            return ConnectAsync(hostname, port, serverKey, null);
        }

        /// <summary>
        /// Connects the TCP Client asynchronously.
        /// </summary>
        /// <param name="hostname">IP address or hostname.</param>
        /// <param name="port">TCP port to connect.</param>
        /// <param name="serverKey">Public RSA key of the server.</param>
        /// <param name="progress">Reports the progress of connection build up.</param>
        /// <returns></returns>
        public async Task ConnectAsync(string hostname, int port, string serverKey, IProgress<ConnectionState> progress)
        {
            progress?.Report(ConnectionState.Stalled);
            
            // check args
            if (string.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), port, "You must provide a valid port number");
            if (string.IsNullOrEmpty(serverKey)) throw new ArgumentNullException(nameof(serverKey));

            progress?.Report(ConnectionState.DnsLookup);
            IPAddress[] ipaddr = await Dns.GetHostAddressesAsync(hostname);
            TcpClient tcp = new TcpClient(AddressFamily.InterNetworkV6);
            tcp.Client.DualMode = true;
            progress?.Report(ConnectionState.TcpConnect);
            await tcp.ConnectAsync(ipaddr, port);
            channel = new NetworkChannel(this, tcp.Client);

            // initialize component
            manager = new NetworkManager(this, serverKey);
            handler = new PacketHandlerClient(this);
            base.handler = handler;
            channel.StartThreads();

            // key exchange
            progress?.Report(ConnectionState.Handshake);
            Task s = Task.Run(() => manager.SendPacket(CryptoAlgorithm.None, new P00Handshake(RequestType.DirectPublicKey)));
            manager.GenerateKeys();
            await s;
            await Task.Run(() => manager.SendPacket(CryptoAlgorithm.RSA_2048_OAEP, new P01KeyExchange(manager.AesKey, manager.SendIV,
                manager.ReceiveIV, Constants.VersionNumber, Constants.CompatibilityVersion, latestProduct, oldestProduct)));

            // wait for response
            progress?.Report(ConnectionState.KeyExchange);
            tcs = new TaskCompletionSource<int>();
            await tcs.Task;
            tcs = null;

            progress?.Report(ConnectionState.Finished);
        }

        internal override void OnConnectionEstablished()
        {
            base.OnConnectionEstablished();
            tcs?.SetResult(0);
        }

        /// <summary>
        /// Defines a set of connection states.
        /// </summary>
        public enum ConnectionState
        {
            /// <summary>
            /// A connection build up was requested.
            /// </summary>
            Stalled,
            /// <summary>
            /// The DNS lookup is running.
            /// </summary>
            DnsLookup,
            /// <summary>
            /// The TCP connection build up is running.
            /// </summary>
            TcpConnect,
            /// <summary>
            /// The handshake is being sent to the server.
            /// </summary>
            Handshake,
            /// <summary>
            /// Waiting for a key exchange response.
            /// </summary>
            KeyExchange,
            /// <summary>
            /// The connection build up was finished.
            /// </summary>
            Finished,
        }
    }
}