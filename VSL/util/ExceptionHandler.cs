﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        /// Handles an Exception by closing the connection and releasing all associated resources.
        /// </summary>
        /// <param name="ex">Exception to print.</param>
        /// 
        internal void CloseConnection(Exception ex)
        {
            parent.CloseConnection(ex.Message);
            PrintException(ex);
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
            parent.Logger.E(ex.ToString());
        }
        //  functions>
    }
}