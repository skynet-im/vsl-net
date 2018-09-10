using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VSLTest
{
    public class PenetrationTest
    {
        private Stopwatch stopwatch;
        private volatile bool running = false;
        public int Total { get; private set; }
        public int Done { get; private set; }
        public long ElapsedTime => stopwatch.ElapsedMilliseconds;
        public bool Running => running;

        public PenetrationTest()
        {
            stopwatch = new Stopwatch();
        }

        public Task RunAsync(int count)
        {
            Total = count;
            Done = 0;
            IPAddress address = IPAddress.Parse("::1");
            Random random = new Random();
            stopwatch.Reset();
            return Task.Run(() =>
            {
                running = true;
                stopwatch.Start();
                while (running && Done < Total)
                {
                    try
                    {
                        TcpClient tcp = new TcpClient(AddressFamily.InterNetworkV6);
                        tcp.Connect(address, Program.Port);
                        Random rand = new Random();
                        byte[] buf = new byte[rand.Next(2048)];
                        rand.NextBytes(buf);
                        tcp.Client.Send(buf);
                        tcp.Close();
                        Done++;
                    }
                    catch { }
                }
            });
        }

        public void Stop()
        {
            stopwatch.Stop();
            running = false;
        }
    }
}
