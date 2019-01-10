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
        /// <summary>
        /// Creates a VSL server for the specified client. To start working, call <see cref="Start"/>.
        /// </summary>
        /// <param name="socket">Connected <see cref="Socket"/>.</param>
        /// <param name="settings">Class containing the RSA key and more settings.</param>
        /// <param name="callback">Inferface for event handling callbacks.</param>
        public VSLServer(Socket socket, SocketSettings settings, IVSLCallback callback)
            : this(socket, CreateFakeCache(), settings, callback) { }

        internal VSLServer(Socket socket, MemoryCache<SocketAsyncEventArgs> cache, SocketSettings settings, IVSLCallback callback)
            : base(settings, callback)
        {
            Channel = new NetworkChannel(socket, ExceptionHandler, cache);
            Manager = new NetworkManager(this, Settings.RsaKey);
            Handler = new PacketHandlerServer(this, Settings.LatestProductVersion, Settings.OldestProductVersion);
            StartReceiveLoop();
        }
    }
}