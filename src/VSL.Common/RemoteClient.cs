using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace VSL.Common
{
    public class RemoteClient : IVSLCallback
    {
        protected VSLServer vsl;

        public RemoteClient()
        {
            ImmutableInterlocked.Update(ref Library.Clients, list => list.Add(this));
        }

        public virtual void OnInstanceCreated(VSLSocket socket)
        {
            vsl = (VSLServer)socket;
        }

        public virtual Task OnConnectionEstablished()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnPacketReceived(byte id, byte[] content)
        {
            return Task.CompletedTask;
        }

        public virtual void OnConnectionClosed(ConnectionCloseReason reason, string message, Exception exception)
        {
            vsl.Dispose();
            ImmutableInterlocked.Update(ref Library.Clients, list => list.Remove(this));
        }

        public Task<bool> SendPacket(byte id, byte[] content)
        {
            return vsl.SendPacketAsync(id, content);
        }

        public bool CloseConnection(string message, Exception exception = null)
        {
            return vsl.CloseConnection(message, exception);
        }
    }
}
