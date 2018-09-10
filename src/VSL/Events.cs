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
        /// <summary>
        /// Initializes a new instance of the PacketReceivedEventArgs class
        /// </summary>
        /// <param name="id">Packet ID</param>
        /// <param name="content">Packet content</param>
        internal PacketReceivedEventArgs(byte id, byte[] content)
        {
            ID = id;
            Content = content;
        }
        /// <summary>
        /// Gets the ID of the received packet
        /// </summary>
        public byte ID { get; }
        /// <summary>
        /// Gets the content of the received packet
        /// </summary>
        public byte[] Content { get; }
    }
    /// <summary>
    /// Event data when the VSL connection was closed
    /// </summary>
    public class ConnectionClosedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the ConnectionClosedEventArgs class.
        /// </summary>
        /// <param name="reason">Reason for connection interruption.</param>
        /// <param name="receivedBytes">Count of received bytes of this session.</param>
        /// <param name="sentBytes">Count of sent bytes of this session.</param>
        internal ConnectionClosedEventArgs(string reason, long receivedBytes, long sentBytes)
        {
            Reason = reason;
            ReceivedBytes = receivedBytes;
            SentBytes = sentBytes;
        }
        /// <summary>
        /// Gets the reason why the connection was interrupted.
        /// </summary>
        public string Reason { get; }
        /// <summary>
        /// Gets the count of received bytes of this session.
        /// </summary>
        public long ReceivedBytes { get; }
        /// <summary>
        /// Gets the count of sent bytes of this session.
        /// </summary>
        public long SentBytes { get; }
    }
    /// <summary>
    /// Event data for a VSL log.
    /// </summary>
    public class LoggedMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the LoggedMessageEventArgs class.
        /// </summary>
        /// <param name="type">Type of the log message.</param>
        /// <param name="text">Text of the log message.</param>
        internal LoggedMessageEventArgs(Logger.LogType type, string text)
        {
            Type = type;
            Text = text;
        }
        /// <summary>
        /// Gets the type of the related log message.
        /// </summary>
        public Logger.LogType Type { get; }
        /// <summary>
        /// Gets the text of the related log message.
        /// </summary>
        public string Text { get; }
    }
}