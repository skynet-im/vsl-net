using System;
using System.Net;
using System.Net.Sockets;

namespace VSL
{
    /// <summary>
    /// A high-performance tcp listener to accept VSL client connects.
    /// </summary>
    public class VSLListener
    {
        private readonly Socket[] sockets;
        private readonly MemoryCache<SocketAsyncEventArgs> cache;
        private readonly Action<VSLServer> acceptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSLListener"/> class for the given endpoints.
        /// </summary>
        /// <param name="addresses">Endpoints to listen on.</param>
        /// <param name="settings">Default settings for accepted clients.</param>
        /// <param name="acceptor">Callback to execute before starting the accepted <see cref="VSLServer"/> objects.</param>
        public VSLListener(IPEndPoint[] addresses, SocketSettings settings, Action<VSLServer> acceptor)
        {
            sockets = new Socket[addresses.Length];
            for (int i = 0; i < addresses.Length; i++)
            {
                sockets[i] = new Socket(addresses[i].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sockets[i].Bind(addresses[i]);
            }
            cache = new MemoryCache<SocketAsyncEventArgs>(128, Constructor, x => x.Buffer.Length == Settings.ReceiveSendBufferSize, x => x.Dispose());
            this.Settings = settings;
            this.acceptor = acceptor;
        }

        /// <summary>
        /// Gets or sets the maximum count of pending client connect requests.
        /// </summary>
        public int Backlog { get; set; } = 128;

        /// <summary>
        /// Gets or sets the maximum count of <see cref="SocketAsyncEventArgs"/> cached for receive and send operations.
        /// </summary>
        public int CacheCapacity { get => cache.Capacity; set => cache.Capacity = value; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public SocketSettings Settings { get; }

        /// <summary>
        /// Starts listening on the specified endpoints.
        /// </summary>
        public void Start()
        {
            foreach (Socket socket in sockets)
            {
                socket.Listen(Backlog);
                var args = new SocketAsyncEventArgs();
                args.Completed += Accept_Completed;
                socket.AcceptAsync(args);
            }
        }

        /// <summary>
        /// Stops all listeners.
        /// </summary>
        public void Stop()
        {
            foreach (Socket socket in sockets)
            {
                socket.Dispose();
            }
        }

        private SocketAsyncEventArgs Constructor()
        {
            int length = Settings.ReceiveSendBufferSize;
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.SetBuffer(new byte[length], 0, length);
            return e;
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket accepted = e.AcceptSocket;
                e.AcceptSocket = null;
                ((Socket)sender).AcceptAsync(e);

                VSLServer server = new VSLServer(accepted, cache, Settings);
                acceptor(server);
                server.Start();
            }
            else
            {
#if DEBUG
                Console.WriteLine("SocketError." + e.SocketError);
#endif
                e.Dispose();
            }
        }
    }
}
