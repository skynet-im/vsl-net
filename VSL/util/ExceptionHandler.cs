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
        internal void HandleInvalidOperationException(InvalidOperationException ex)
        {
            parent.channel.CloseConnection("Invalid packet received");
            Console.WriteLine("Invalid packet received: " + ex.ToString());
        }
        internal void HandleReceiveTimeoutException(TimeoutException ex)
        {
            parent.channel.CloseConnection("Timeout while waiting for more data");
            Console.WriteLine("Timeout while waiting for more data: " + ex.ToString());
        }
        //  functions>
    }
}