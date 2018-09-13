using System;
using System.Net.Sockets;
using VSL.Network;

namespace VSL
{
    /// <summary>
    /// The server implementation of a VSL socket
    /// </summary>
    public sealed class VSLServer : VSLSocket
    {
        // <constructor
        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="socket">Connected <see cref="Socket"/>.</param>
        /// <param name="latestProduct">The application version.</param>
        /// <param name="oldestProduct">The oldest supported version.</param>
        /// <param name="keypair">The RSA-keypair of the server application.</param>
        public VSLServer(Socket socket, ushort latestProduct, ushort oldestProduct, string keypair)
        {
            InitializeComponent();

            Channel = new NetworkChannel(socket, ExceptionHandler);
            Manager = new NetworkManager(this, keypair);
            Handler = new PacketHandlerServer(this, latestProduct, oldestProduct);
        }
        //  constructor>

        // <functions
        /// <summary>
        /// Starts all threads for receiving and process packets.
        /// </summary>
        public void Start()
        {
            StartReceiveLoop();
        }
        //  functions>
    }
}