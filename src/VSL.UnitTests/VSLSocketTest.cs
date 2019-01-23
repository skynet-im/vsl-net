using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VSL.Crypt;
using VSL.Network;

namespace VSL.UnitTests
{
    [TestClass]
    public class VSLSocketTest
    {
        [TestMethod]
        public async Task TestThrowing()
        {
            SocketSettings settings = new SocketSettings() { CatchApplicationExceptions = false, RsaKey = RsaStatic.GenerateKeyPairParams() };
            IVSLCallback callback = new FakeCallback();
            FakeSocket socket = new FakeSocket(settings, callback);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => socket.OnPacketReceived(0, null));
        }

        [TestMethod]
        public async Task TestCatching()
        {
            SocketSettings settings = new SocketSettings() { CatchApplicationExceptions = true, RsaKey = RsaStatic.GenerateKeyPairParams() };
            IVSLCallback callback = new FakeCallback();
            FakeSocket socket = new FakeSocket(settings, callback);
            Assert.IsFalse(await socket.OnPacketReceived(0, null));
        }

        public class FakeSocket : VSLSocket
        {
            public FakeSocket(SocketSettings settings, IVSLCallback callback) : base(settings, callback)
            {
                Channel = new NetworkChannel(new Socket(SocketType.Stream, ProtocolType.Tcp), ExceptionHandler, CreateFakeCache());
            }
        }

        public class FakeCallback : IVSLCallback
        {
            public void OnInstanceCreated(VSLSocket socket) { }

            public Task OnConnectionEstablished() => Task.CompletedTask;

            public Task OnPacketReceived(byte id, byte[] content)
            {
                throw new InvalidOperationException();
            }

            public void OnConnectionClosed(ConnectionCloseReason reason, string message, Exception exception) { }
        }
    }
}
