using System;

namespace VSL
{
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