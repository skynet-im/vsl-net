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
        internal VSLSocket parent;
        //  fields>
        // <constructor
        internal ExceptionHandler(VSLSocket parent)
        {
            this.parent = parent;
        }
        //  constructor>
        // <functions
        /// <summary>
        /// Handles an exception caused by invalid packets
        /// </summary>
        /// <param name="ex">Exception to print</param>
        internal void HandleArgumentOutOfRangeException(ArgumentOutOfRangeException ex)
        {
            parent.CloseConnection("Argument out of range -> invalid packet");
            Console.WriteLine("Argument out of range -> invalid packet: " + ex.ToString());
        }
        /// <summary>
        /// Handles an exception caused by wrong keys
        /// </summary>
        /// <param name="ex">Exception to print</param>
        internal void HandleCryptographicException(System.Security.Cryptography.CryptographicException ex)
        {
            parent.CloseConnection("Cryptographic operation failed due to wrong keys");
            Console.WriteLine("Cryptographic operation failed due to wrong keys: " + ex.ToString());
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
            parent.CloseConnection("Method is not supported supported by this VSL version -> invalid operation");
            Console.WriteLine("Method is not supported supported by this VSL version -> invalid operation: " + ex.ToString());
        }
        internal void HandleReceiveTimeoutException(TimeoutException ex)
        {
            parent.CloseConnection("Timeout while waiting for more data");
            Console.WriteLine("Timeout while waiting for more data: " + ex.ToString());
        }
        //  functions>
    }
}