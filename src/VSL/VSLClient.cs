using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using VSL.Network;
using VSL.Packet;

namespace VSL
{
    /// <summary>
    /// The client implementation of a VSL socket.
    /// </summary>
    public sealed class VSLClient : VSLSocket
    {
        private TaskCompletionSource<bool> tcs;

        /// <summary>
        /// Creates a VSL Client that has to be connected.
        /// </summary>
        /// <param name="settings">Class containing the RSA key and more settings.</param>
        public VSLClient(SocketSettings settings, IVSLCallback callback) : base(settings, callback)
        {
        }

        /// <summary>
        /// Connects the TCP Client asynchronously.
        /// </summary>
        /// <param name="hostname">IP address or hostname.</param>
        /// <param name="port">TCP port to connect.</param>
        public Task<bool> ConnectAsync(string hostname, int port)
        {
            return ConnectAsync(hostname, port, null);
        }

        /// <summary>
        /// Connects the TCP Client asynchronously.
        /// </summary>
        /// <param name="hostname">IP address or hostname.</param>
        /// <param name="port">TCP port to connect.</param>
        /// <param name="progress">Reports the progress of connection build up.</param>
        public async Task<bool> ConnectAsync(string hostname, int port, IProgress<ConnectionState> progress)
        {
            progress?.Report(ConnectionState.Stalled);

            // check args
            if (string.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), port, "You must provide a valid port number");

            progress?.Report(ConnectionState.DnsLookup);
            IPAddress[] ipaddr = await Dns.GetHostAddressesAsync(hostname);
            TcpClient tcp = new TcpClient(AddressFamily.InterNetworkV6);
            tcp.Client.DualMode = true;
            progress?.Report(ConnectionState.TcpConnect);
            await tcp.ConnectAsync(ipaddr, port);
            Channel = new NetworkChannel(tcp.Client, ExceptionHandler, CreateFakeCache());

            // initialize component
            Manager = new NetworkManager(this, Settings.RsaKey);
            Handler = new PacketHandlerClient(this);
            StartReceiveLoop();

            // key exchange
            progress?.Report(ConnectionState.Handshake);
            Task s = Manager.SendPacketAsync(CryptoAlgorithm.None, new P00Handshake(RequestType.DirectPublicKey));
            Manager.GenerateKeys();
            await s;
            await Manager.SendPacketAsync(CryptoAlgorithm.RSA_2048_OAEP, new P01KeyExchange(Manager.AesKey, Manager.SendIV,
                Manager.ReceiveIV, Constants.ProtocolVersion, Constants.CompatibilityVersion, Settings.LatestProductVersion, Settings.OldestProductVersion));

            // wait for response
            progress?.Report(ConnectionState.KeyExchange);
            tcs = new TaskCompletionSource<bool>();
            bool result = await tcs.Task;
            tcs = null;

            progress?.Report(result ? ConnectionState.Finished : ConnectionState.Canceled);
            return result;
        }

        internal override Task OnConnectionEstablished()
        {
            tcs?.SetResult(true);
            return base.OnConnectionEstablished();
        }

        internal override Task OnConnectionClosed(ConnectionCloseReason reason, string message, Exception exception)
        {
            tcs?.SetResult(false);
            return base.OnConnectionClosed(reason, message, exception);
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
            /// <summary>
            /// The connection was closed before a secure channel had been established.
            /// </summary>
            Canceled
        }
    }
}