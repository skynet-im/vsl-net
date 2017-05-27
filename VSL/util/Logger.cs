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
        /// <summary>
        /// Gets or sets a value indicating whether debug messages are printed in the console.
        /// </summary>
        public bool DebugMessages { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether exception messages are printed in the console.
        /// </summary>
        public bool ExceptionMessages { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether information messages are printed in the console.
        /// </summary>
        public bool InfoMessages { get; set; } = false;

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
            if (DebugMessages)
                Console.WriteLine("[VSL Debug] " + s);
        }
        /// <summary>
        /// Prints an exception message
        /// </summary>
        internal void e(string s)
        {
            if (ExceptionMessages)
                Console.WriteLine("[VSL Exception] " + s);
        }
        /// <summary>
        /// Prints an information message
        /// </summary>
        internal void i(string s)
        {
            if (InfoMessages)
                Console.WriteLine("[VSL Info] " + s);
        }
    }
}