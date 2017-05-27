using System;
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
        private ConcurrentQueue<Exception> queue;
        private CancellationTokenSource cts;
        private CancellationToken ct;
        //  fields>
        // <constructor
        internal ExceptionHandler(VSLSocket parent)
        {
            this.parent = parent;
            queue = new ConcurrentQueue<Exception>();
            cts = new CancellationTokenSource();
            ct = cts.Token;
            Task et = ExceptionThrower();
        }
        //  constructor>
        // <functions
        internal void StopTasks()
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }

        private async Task ExceptionThrower()
        {
            while (!ct.IsCancellationRequested)
            {
                if (queue.Count > 0)
                {
                    Exception ex;
                    bool success = queue.TryDequeue(out ex);
                    if (success)
                    {
                        parent.CloseConnection(ex.Message);
                        PrintException(ex);
                    }
                }
                else
                    await Task.Delay(10, ct);
            }
        }

        /// <summary>
        /// Handles an Exception by closing the connection and releasing all associated resources.
        /// </summary>
        /// <param name="ex">Exception to print.</param>
        /// <param name="invoke">Redirects exceptions thrown by background threads to the UI thread.</param>
        internal void HandleException(Exception ex, bool invoke = false)
        {
            if (!invoke)
            {
                parent.CloseConnection(ex.Message);
                PrintException(ex);
            }
            else
                queue.Enqueue(ex);
        }

        /// <summary>
        /// Prints an Exception.
        /// </summary>
        internal void PrintException(Exception ex)
        {
            parent.Logger.e(ex.ToString());
        }
        //  functions>
    }
}