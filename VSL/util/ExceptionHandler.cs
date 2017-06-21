using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSL
{
    internal class ExceptionHandler : IDisposable
    {
        // <fields
        internal VSLSocket parent;
        private ConcurrentQueue<Action> queue;
        private CancellationTokenSource cts;
        private CancellationToken ct;
        //  fields>
        // <constructor
        internal ExceptionHandler(VSLSocket parent)
        {
            this.parent = parent;
            queue = new ConcurrentQueue<Action>();
            cts = new CancellationTokenSource();
            ct = cts.Token;
            Task et = MainThreadInvoker();
        }
        //  constructor>
        // <functions
        internal void StopTasks()
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }

        private async Task MainThreadInvoker()
        {
            while (!ct.IsCancellationRequested)
            {
                if (queue.Count > 0)
                {
                    Action work;
                    bool success = queue.TryDequeue(out work);
                    if (success)
                    {
                        work.Invoke();
                    }
                }
                else
                    await Task.Delay(parent.SleepTime, ct);
            }
        }

        /// <summary>
        /// Handles an Exception by closing the connection and releasing all associated resources.
        /// </summary>
        /// <param name="ex">Exception to print.</param>
        /// 
        internal void CloseConnection(Exception ex)
        {
            Action myDelegate = new Action(delegate
            {
                PrintException(ex);
                parent.CloseConnection(ex.Message);
            });
            queue.Enqueue(myDelegate);
        }

        /// <summary>
        /// Handles an Exception by cancelling the current file transfer and releasing all associated resources.
        /// </summary>
        /// <param name="ex"></param>
        internal void CancelFileTransfer(Exception ex)
        {
            Action myDelegate = new Action(delegate
            {
                PrintException(ex);
                parent.FileTransfer.Cancel();
            });
            queue.Enqueue(myDelegate);
        }

        /// <summary>
        /// Prints an Exception.
        /// </summary>
        internal void PrintException(Exception ex)
        {
            parent.Logger.e(ex.ToString());
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    cts.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ExceptionHandler() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        //  functions>
    }
}