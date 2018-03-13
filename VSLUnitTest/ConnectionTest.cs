using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSL;
using VSLTest;

namespace VSLUnitTest
{
    [TestClass]
    public class ConnectionTest
    {
        VSLClient client;
        Server server;

        public void Initialize()
        {
            server = new Server(Program.Port, Program.Keypair);
            server.Start(true, false);
            client = new VSLClient(0, 0, ThreadManager.CreateThreadPool());
            client.Logger.PrintDebugMessages = true;
            client.Logger.PrintExceptionMessages = true;
            client.Logger.PrintInfoMessages = true;
            client.Logger.PrintUncaughtExceptions = true;
        }

        public async Task Connect()
        {
            ManualResetEventSlim handle = new ManualResetEventSlim();
            client.ConnectionEstablished += (sender, e) =>
            {
                Console.WriteLine("Handle was set");
                handle.Set();
            };
            await client.ConnectAsync("127.0.0.1", Program.Port, Program.PublicKey);
            if (!await Task.Run<bool>(() => handle.Wait(3000)))
                Console.WriteLine("Waiting took too long");
            handle.Dispose();
        }

        [TestMethod]
        public async Task TestConnection()
        {
            Initialize();
            await Connect();
            Cleanup();
        }

        [TestMethod]
        public async Task TestSendPacket()
        {
            Initialize();
            await Connect();
            client.SendPacket(7, new byte[] { 87, 5, 31 });
            Cleanup();
        }

        [TestMethod]
        public void TestFileTransfer()
        {

        }

        [TestMethod]
        public void TestFileTransferEncrypted()
        {

        }

        public void Cleanup()
        {
            server.Stop();
            client.Dispose();
        }
    }
}
