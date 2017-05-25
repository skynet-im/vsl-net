using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal class ExceptionHandler
    {
        // <fields
        internal VSLSocket parent;
        private ConcurrentQueue<CrossThreadException> queue;
        //  fields>
        // <constructor
        internal ExceptionHandler(VSLSocket parent)
        {
            this.parent = parent;
            queue = new ConcurrentQueue<CrossThreadException>();
            Task et = ExceptionThrower();
        }
        //  constructor>
        // <functions
        private async Task ExceptionThrower()
        {
            while (true)
            {
                if (queue.Count > 0)
                {
                    CrossThreadException ex;
                    bool success = queue.TryDequeue(out ex);
                    if (success)
                    {
                        parent.CloseConnection(ex.Message);
                        parent.Logger.e(ex.Message + ": " + ex.Exception.ToString());
                    }
                }
                else
                    await Task.Delay(10);
            }
        }
        /// <summary>
        /// Handles an exception caused by invalid packets
        /// </summary>
        /// <param name="ex">Exception to print</param>
        internal void HandleArgumentOutOfRangeException(ArgumentOutOfRangeException ex)
        {
            parent.CloseConnection("Argument out of range -> invalid packet");
            Console.WriteLine("Argument out of range -> invalid packet: " + ex.ToString());
        }
        internal void HandleInvalidCastException(InvalidCastException ex)
        {
            parent.CloseConnection("Enum cast failed -> algorithm or feature not supported");
            Console.WriteLine("Enum cast failed -> algorithm or feature not supported: " + ex.ToString());
        }
        internal void HandleInvalidOperationException(InvalidOperationException ex)
        {
            parent.CloseConnection("Invalid packet received");
            Console.WriteLine("Invalid packet received: " + ex.ToString());
        }
        internal void HandleNotImplementedException(NotImplementedException ex)
        {
            parent.CloseConnection("Method is not implemented -> invalid operation");
            Console.WriteLine("Method is not implemented -> invalid operation: " + ex.ToString());
        }
        internal void HandleNotSupportedException(NotSupportedException ex)
        {
            parent.CloseConnection("Method is not supported by this VSL version -> invalid operation");
            Console.WriteLine("Method is not supported by this VSL version -> invalid operation: " + ex.ToString());
        }
        internal void HandleReceiveTimeoutException(TimeoutException ex)
        {
            parent.CloseConnection("Timeout while waiting for more data");
            Console.WriteLine("Timeout while waiting for more data: " + ex.ToString());
        }
        /// <summary>
        /// Handles a CryptographicException by closing the connection and releasing all associated resources.
        /// </summary>
        /// <param name="ex">Exception to print.</param>
        /// <param name="invoke">Redirects exceptions thrown by background threads to the UI thread.</param>
        internal void HandleException(System.Security.Cryptography.CryptographicException ex, bool invoke = false)
        {
            string message = "Cryptographic operation failed";
            if (!invoke)
            {
                parent.CloseConnection(message);
                parent.Logger.e(message + ": " + ex.ToString());
            }
            else
                queue.Enqueue(new CrossThreadException(ex, message));
        }
        /// <summary>
        /// Handles a SocketException by closing the connection and releasing all associated resources.
        /// </summary>
        /// <param name="ex">Exception to print.</param>
        /// <param name="invoke">Redirects exceptions thrown by background threads to the UI thread.</param>
        internal void HandleException(System.Net.Sockets.SocketException ex, bool invoke = false)
        {
            string message = "Socket was closed";
            if (!invoke)
            {
                parent.CloseConnection(message);
                parent.Logger.e(message + ": " + ex.ToString());
            }
            else
                queue.Enqueue(new CrossThreadException(ex, message));
        }
        //  functions>
        private class CrossThreadException
        {
            public Exception Exception { get; }
            public string Message { get; }
            public CrossThreadException(Exception ex, string message)
            {
                Exception = ex;
                Message = message;
            }
        }
    }
}