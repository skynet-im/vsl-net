using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL;

namespace VSLTest
{
    public class LocalClient : IVSLCallback
    {
        private VSLClient vsl;

        public void OnInstanceCreated(VSLSocket socket)
        {
            vsl = (VSLClient)socket;
        }

        public Task OnConnectionEstablished()
        {
            return Task.CompletedTask;
        }

        public Task OnPacketReceived(byte id, byte[] content)
        {
            return Task.CompletedTask;
        }

        public void OnConnectionClosed(ConnectionCloseReason reason, string message, Exception exception)
        {

        }
    }
}
