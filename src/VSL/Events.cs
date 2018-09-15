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
            Id = id;
            Content = content;
        }
        /// <summary>
        /// Gets the ID of the received packet
        /// </summary>
        public byte Id { get; }
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
        /// <param name="message">Exception message.</param>
        /// <param name="ex">Exception causing connection closure.</param>
        internal ConnectionClosedEventArgs(ConnectionCloseReason reason, string message, Exception ex)
        {
            Reason = reason;
            Message = message;
            Exception = ex;
        }
        /// <summary>
        /// Gets the reason for connection closure.
        /// </summary>
        public ConnectionCloseReason Reason { get; }
        /// <summary>
        /// Gets the reason why the connection was interrupted.
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// Gets the exception that caused connection closure.
        /// </summary>
        public Exception Exception { get; }
    }

    public enum ConnectionCloseReason
    {
        None,
        SocketError,
        InternalError,
        UserCodeError,
        UserRequested
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