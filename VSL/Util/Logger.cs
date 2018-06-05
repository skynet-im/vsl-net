using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        // <properties
        #region debug
        /// <summary>
        /// Gets or sets a value indicating whether debug messages are directly printed in the console.
        /// </summary>
        public bool PrintDebugMessages { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether the LoggedMessage event is raised for debug messages.
        /// </summary>
        public bool InvokeDebugMessages { get; set; } = true;
        internal bool InitD => PrintDebugMessages || InvokeDebugMessages;
        #endregion
        #region exception
        /// <summary>
        /// Gets or sets a value indicating whether exception messages are printed in the console.
        /// </summary>
        public bool PrintExceptionMessages { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether the LoggedMessage event is raised for exception messages.
        /// </summary>
        public bool InvokeExceptionMessages { get; set; } = true;
        internal bool InitE => PrintExceptionMessages || InvokeExceptionMessages;
        #endregion
        #region info
        /// <summary>
        /// Gets or sets a value indicating whether information messages are printed in the console.
        /// </summary>
        public bool PrintInfoMessages { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether the LoggedMessage event is raised for info messages.
        /// </summary>
        public bool InvokeInfoMessages { get; set; } = true;
        internal bool InitI => PrintInfoMessages || InvokeInfoMessages;
        #endregion
        /// <summary>
        /// Gets or sets a value indicating whether uncaught exceptions are printed in the console.
        /// </summary>
        public bool PrintUncaughtExceptions { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether prefixes for types are written in events.
        /// </summary>
        public bool WritePrefix { get; set; } = true;
        //  properties>
        // <events
        /// <summary>
        /// The LoggedMessage event occurs when a new message has been logged and invoking its type is enabled.
        /// </summary>
        public event EventHandler<LoggedMessageEventArgs> LoggedMessage;
        private void OnLoggedMessage(string text, LogType type)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                if (WritePrefix) text = PrependPrefix(text, type);
                LoggedMessage?.Invoke(this, new LoggedMessageEventArgs(type, text));
            });
        }
        //  events>
        private void PrintMessage(string text, LogType type)
        {
            text = PrependPrefix(text, type);
#if WINDOWS_UWP
            System.Diagnostics.Debug.WriteLine(text);
#else
            Console.WriteLine(text);
#endif
        }

        private string PrependPrefix(string text, LogType type)
        {
#if DEBUG
            string rel;
            if (parent is VSLServer)
                rel = "Server ";
            else if (parent is VSLClient)
                rel = "Client ";
            else
                rel = "";
            return "[VSL " + rel + type.ToString() + "] " + text;
#else
            return "[VSL " + type.ToString() + "] " + text;
#endif
        }

        /// <summary>
        /// Prints a debug message
        /// </summary>
        internal void D(string s)
        {
            if (PrintDebugMessages)
                PrintMessage(s, LogType.Debug);
            if (InvokeDebugMessages)
                OnLoggedMessage(s, LogType.Debug);
        }
        /// <summary>
        /// Prints an exception message
        /// </summary>
        internal void E(string s)
        {
            if (PrintExceptionMessages)
                PrintMessage(s, LogType.Exception);
            if (InvokeExceptionMessages)
                OnLoggedMessage(s, LogType.Exception);
        }
        /// <summary>
        /// Prints an information message
        /// </summary>
        internal void I(string s)
        {
            if (PrintInfoMessages)
                PrintMessage(s, LogType.Info);
            if (InvokeInfoMessages)
                OnLoggedMessage(s, LogType.Info);
        }
        internal void Uncaught(string s)
        {
            if (PrintUncaughtExceptions)
                PrintMessage(s, LogType.UncaughtException);
            OnLoggedMessage(s, LogType.UncaughtException);
        }
        /// <summary>
        /// Specifies the type of a log output.
        /// </summary>
        public enum LogType : byte
        {
            /// <summary>
            /// Detailed information for debugging such as packets, AES-blocks, etc.
            /// </summary>
            Debug,
            /// <summary>
            /// Information about occuring exceptions.
            /// </summary>
            Exception,
            /// <summary>
            /// Basic information about events like connection build-up, disconnect or file transfer.
            /// </summary>
            Info,
            /// <summary>
            /// Information about occuring unexpected fatal exceptions.
            /// </summary>
            UncaughtException
        }
    }
}