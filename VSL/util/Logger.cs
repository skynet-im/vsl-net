using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    /// <summary>
    /// Responsible for all console output of VSL
    /// </summary>
    public class Logger
    {
        private VSLSocket parent;
        internal Logger(VSLSocket parent)
        {
            this.parent = parent;
        }
        /// <summary>
        /// Prints a debug message
        /// </summary>
        internal void d(string s)
        {
            Console.WriteLine("[VSL Debug] " + s);
        }
        /// <summary>
        /// Prints an exception message
        /// </summary>
        internal void e(string s)
        {
            Console.WriteLine("[VSL Exception] " + s);
        }
        /// <summary>
        /// Prints an information message
        /// </summary>
        internal void i(string s)
        {
            Console.WriteLine("[VSL Info] " + s);
        }
    }
}