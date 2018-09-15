﻿using System;
using System.Net.Sockets;

namespace VSL
{
    internal class ExceptionHandler
    {
        // <fields
        private VSLSocket parent;
        //  fields>
        // <constructor
        internal ExceptionHandler(VSLSocket parent)
        {
            this.parent = parent;
        }
        //  constructor>
        // <functions
        /// <summary>
        /// Handles an Exception by closing the connection and releasing all associated resources.
        /// </summary>
        /// <param name="ex">Exception to print.</param>
        internal void CloseConnection(Exception ex)
        {
            string msg = $"A {ex.GetType()} has been thrown in internal code";
            parent.CloseInternal(ConnectionCloseReason.InternalError, msg, ex);
        }

        internal void CloseConnection(SocketError err, SocketAsyncOperation operation)
        {
            string msg = $"A socket error occured while trying to {operation}: {err}";
            parent.CloseInternal(ConnectionCloseReason.SocketError, msg, null);
        }

        internal void CloseConnection(string errorCode, string message, string className, string memberName)
        {
            string msg = $"An internal error occured - {errorCode}: {message}{Environment.NewLine}\tat {className}.{memberName}";
            parent.CloseInternal(ConnectionCloseReason.InternalError, msg, null);
        }

        internal void CloseUncaught(Exception ex)
        {
            string msg = $"A {ex.GetType()} was thrown in user code as has not been handled";
            parent.CloseInternal(ConnectionCloseReason.UserCodeError, msg, ex);
        }
        //  functions>
    }
}