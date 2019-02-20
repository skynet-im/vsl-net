using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using VSL;

namespace VSLTest
{
    public abstract class PenetrationTest
    {
        protected Stopwatch stopwatch;
        protected Random random;
        protected IPAddress address;
        protected volatile bool running = false;
        public int Total;
        public int Done;
        public int Errors;
        public long ElapsedTime => stopwatch.ElapsedMilliseconds;
        public bool Running => running;

        public PenetrationTest()
        {
            stopwatch = new Stopwatch();
            random = new Random();
            address = IPAddress.Parse("::1");
        }

        public Task RunAsync(int count)
        {
            Total = count;
            Done = 0;
            Errors = 0;
            stopwatch.Reset();
            return RunInternal();
        }

        public void Stop()
        {
            stopwatch.Stop();
            running = false;
        }

        protected abstract Task RunInternal();
    }

    public class ConnectTest : PenetrationTest
    {
        protected override Task RunInternal()
        {
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
                    catch
                    {
                        Errors++;
                    }
                }
            });
        }
    }

    public class SendTest : PenetrationTest
    {
        protected override Task RunInternal()
        {
            return Task.Run(async () =>
            {
                SocketSettings settings = new SocketSettings
                {
                    CatchApplicationExceptions = false,
                    RsaXmlKey = Program.PublicKey
                };
                LocalClient local = new LocalClient();
                VSLClient client = new VSLClient(settings, local);
                await client.ConnectAsync("::1", Program.Port);
                running = true;
                stopwatch.Start();
                async Task inner()
                {
                    while (running && Done < Total)
                    {
                        byte[] buf = new byte[random.Next(2048)];
                        random.NextBytes(buf);
                        await client.SendPacketAsync(0, buf);
                        Interlocked.Increment(ref Done);
                    }
                }
                await Task.WhenAll(Enumerable.Range(0, 4).Select(x => inner()));
            });
        }
    }
}
