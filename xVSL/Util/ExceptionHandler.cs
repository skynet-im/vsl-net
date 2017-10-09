using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (parent.Logger.InitI)
                parent.Logger.I("Connection was forcibly closed by VSL: " + ex.GetType().Name);
            PrintException(ex);
            CloseInternal(ex.ToString());
        }

        internal void CloseConnection(System.Net.Sockets.SocketException ex)
        {
            if (parent.Logger.InitI)
                parent.Logger.I("Connection was interrupted");
            PrintException(ex);
            CloseInternal(ex.ToString());
        }

        internal void CloseConnection(string errorcode, string message)
        {
            if (parent.Logger.InitI)
                parent.Logger.I("Connection was forcibly closed by VSL: " + errorcode);
            if (parent.Logger.InitE)
                parent.Logger.E("Internal error (" + errorcode + "): " + message);
            CloseInternal(message);
        }

        internal void CloseUncaught(Exception ex)
        {
            parent.Logger.Uncaught("A fatal unexpected error occured: " + ex.ToString());
            CloseInternal(ex.ToString());
        }

        /// <summary>
        /// Handles an Exception by cancelling the current file transfer and releasing all associated resources.
        /// </summary>
        /// <param name="ex"></param>
        internal void CancelFileTransfer(Exception ex)
        {
            PrintException(ex);
            parent.FileTransfer.Cancel();
        }

        /// <summary>
        /// Prints an Exception.
        /// </summary>
        internal void PrintException(Exception ex)
        {
            if (parent.Logger.InitE)
                parent.Logger.E(ex.ToString());
        }

        /// <summary>
        /// Calls VSLSocket.CloseInternal or VSLSocket.CloseInternalAsync dependent on the platform that is used.
        /// </summary>
        /// <param name="exception">The exception text to share in the related event.</param>
#if WINDOWS_UWP
        private async void CloseInternal(string exception)
        {
            await parent.CloseInternalAsync(exception);
        }
#else
        private void CloseInternal(string exception)
        {
            parent.CloseInternal(exception);
        }
#endif
        //  functions>
    }
}