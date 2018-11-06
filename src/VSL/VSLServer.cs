using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using VSL.Crypt;
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
        /// <param name="settings">Class containing the RSA key and more settings.</param>
        public VSLServer(Socket socket, SocketSettings settings)
            : this(socket, CreateFakeCache(), settings) { }

        internal VSLServer(Socket socket, MemoryCache<SocketAsyncEventArgs> cache, SocketSettings settings) : base(settings)
        {
            Channel = new NetworkChannel(socket, ExceptionHandler, cache);
            Manager = new NetworkManager(this, Settings.RsaKey);
            Handler = new PacketHandlerServer(this, Settings.LatestProductVersion, Settings.OldestProductVersion);
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