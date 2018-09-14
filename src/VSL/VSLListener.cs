using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace VSL
{
    public class VSLListener
    {
        private readonly Socket[] sockets;

        public VSLListener(IPEndPoint[] addresses)
        {
            sockets = new Socket[addresses.Length];
            for (int i = 0; i < addresses.Length; i++)
            {
                sockets[i] = new Socket(addresses[i].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sockets[i].Bind(addresses[i]);
            }
        }

        /// <summary>
        /// The maximum count of pending client connect requests.
        /// </summary>
        public int Backlog { get; set; } = 128;

        public void Start()
        {
            foreach (Socket socket in sockets)
            {
                socket.Listen(Backlog);

            }
        }

        public void Stop()
        {

        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {

        }
    }
}
