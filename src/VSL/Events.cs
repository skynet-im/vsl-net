using System;

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

    /// <summary>
    /// Defines reasons to close a VSL connection.
    /// </summary>
    public enum ConnectionCloseReason
    {
        /// <summary>
        /// The connection has not been closed yet.
        /// </summary>
        None,
        /// <summary>
        /// A socket error occured.
        /// </summary>
        SocketError,
        /// <summary>
        /// An internal error occured.
        /// </summary>
        InternalError,
        /// <summary>
        /// An unhandled exception was thrown in user code.
        /// </summary>
        UserCodeError,
        /// <summary>
        /// The user requested to disconnect.
        /// </summary>
        UserRequested
    }
}