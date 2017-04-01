using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    /// <summary>
    /// Event data when the VSL socket received a packet
    /// </summary>
    public class PacketReceivedEventArgs : EventArgs
    {
        public PacketReceivedEventArgs(byte id, byte[] content)
        {
            ID = id;
            Content = content;
        }
        public byte ID { get; }
        public byte[] Content { get; }
    }
    public class ConnectionClosedEventArgs : EventArgs
    {
        public ConnectionClosedEventArgs(string reason)
        {
            Reason = reason;
        }
        public string Reason { get; }
    }
}