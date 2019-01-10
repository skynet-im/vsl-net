using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    public interface IVSLCallback
    {
        /// <summary>
        /// Handles the creation of a <see cref="VSLSocket"/> instance.
        /// </summary>
        /// <param name="socket">The created instance of type <see cref="VSLClient"/> or <see cref="VSLServer"/>.</param>
        void OnInstanceCreated(VSLSocket socket);
        
        /// <summary>
        /// Handles an established connection.
        /// </summary>
        Task OnConnectionEstablished();

        /// <summary>
        /// Handles a received packet.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        Task OnPacketReceived(byte id, byte[] content);

        /// <summary>
        /// Handles a connection close.
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void OnConnectionClosed(ConnectionCloseReason reason, string message, Exception exception);
    }
}
