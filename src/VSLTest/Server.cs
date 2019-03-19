using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSL;
using VSL.Common;

namespace VSLTest
{
    public class Server
    {
        private readonly int port;
        private readonly string keypair;
        private VSLListener listener;

        public Server(int port, string keypair)
        {
            this.port = port;
            this.keypair = keypair;
        }

        public bool Running { get; private set; }

        public void Start(bool localhost)
        {
            IPEndPoint[] endPoints = {
                new IPEndPoint(localhost ? IPAddress.Loopback : IPAddress.Any, port),
                new IPEndPoint(localhost ? IPAddress.IPv6Loopback : IPAddress.IPv6Any, port)
            };

            SocketSettings settings = new SocketSettings()
            {
                CatchApplicationExceptions = false,
                RsaXmlKey = Library.Keypair
            };

            listener = new VSLListener(endPoints, settings, () =>
            {
                var client = new RemoteClient();
                Interlocked.Increment(ref Program.Connects);
                return client;
            });

            Running = true;
            listener.Start();
        }

        public void Stop()
        {
            Running = false;
            Library.Clients.ForEach(c => c.CloseConnection("Stopping server"));
            listener?.Stop();
        }
    }
}
