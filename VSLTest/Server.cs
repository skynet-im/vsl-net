using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSL;

namespace VSLTest
{
    public class Server
    {
        private int port;
        private string keypair;
        TcpListener listener4;
        TcpListener listener6;

        public Server(int port, string keypair)
        {
            this.port = port;
            this.keypair = keypair;
        }

        public bool Running { get; private set; }

        public void Start(bool localhost, bool useDispatcher)
        {
            var dispatcher = useDispatcher ? System.Windows.Threading.Dispatcher.CurrentDispatcher : null;
            listener4 = new TcpListener(localhost ? IPAddress.Loopback : IPAddress.Any, port);
            listener6 = new TcpListener(localhost ? IPAddress.IPv6Loopback : IPAddress.IPv6Any, port);
            listener4.Start();
            listener6.Start();
            Running = true;
            void waitCallback(object state)
            {
                TcpListener listener = (TcpListener)state;
                while (Running)
                {
                    try
                    {
                        Socket native = listener.AcceptSocket();
                        Client c = new Client(native, dispatcher);
                        Interlocked.Increment(ref Program.Connects);
                    }
                    catch (SocketException) { return; }
                }
            }
            ThreadPool.QueueUserWorkItem(waitCallback, listener4);
            ThreadPool.QueueUserWorkItem(waitCallback, listener6);
        }

        public void Stop()
        {
            Running = false;
            Program.Clients.ParallelForEach((c) => c.CloseConnection("Stopping server"));
            listener4?.Stop();
            listener6?.Stop();
        }
    }
}
