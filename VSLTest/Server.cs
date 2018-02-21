using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSLTest
{
    public class Server
    {
        private int port;
        private string keypair;

        public Server(int port, string keypair)
        {
            this.port = port;
            this.keypair = keypair;
        }

        public bool Running { get; private set; }

        public void Start()
        {
            var dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            TcpListener listener4 = new TcpListener(IPAddress.Loopback, port);
            TcpListener listener6 = new TcpListener(IPAddress.IPv6Loopback, port);
            listener4.Start();
            listener6.Start();
            Running = true;
            void waitCallback(object state)
            {
                TcpListener listener = (TcpListener)state;
                while (Running)
                {
                    Socket native = listener.AcceptSocket();
                    Client c = new Client(native, dispatcher);
                    Interlocked.Increment(ref Program.Connects);
                }
            }
            ThreadPool.QueueUserWorkItem(waitCallback, listener4);
            ThreadPool.QueueUserWorkItem(waitCallback, listener6);
        }

        public void Stop()
        {
            Running = false;
            Program.Clients.ParallelForEach((c) => c.CloseConnection("Stopping server"));
        }
    }
}
