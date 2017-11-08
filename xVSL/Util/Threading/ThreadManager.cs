using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSL.Threading;

namespace VSL
{
    /// <summary>
    /// Manages the event invocation and execution of custom work items in VSL.
    /// </summary>
    public abstract class ThreadManager : IDisposable
    {
        /// <summary>
        /// Creates a ThreadManager that executes work items on the ThreadPool using the <see cref="Timer"/> class.
        /// </summary>
        /// <returns></returns>
        public static ThreadManager CreateThreadPool()
        {
            return new ThreadManagerThreadPool();
        }

        // TODO: Managed thread

        /// <summary>
        /// Creates a ThreadManager that executes work items on the main thread using async/await.
        /// </summary>
        /// <returns></returns>
        public static ThreadManager CreateAsyncAwait()
        {
            return new ThreadManagerAsyncAwait();
        }

        /// <summary>
        /// The underlying <see cref="VSLSocket"/>.
        /// </summary>
        internal protected VSLSocket parent;
        internal protected CancellationTokenSource cts;
        internal protected CancellationToken ct;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadManager"/> class.
        /// </summary>
        /// <param name="mode">The <see cref="AsyncMode"/> to specify how the <see cref="ThreadManager"/> should work.</param>
        internal protected ThreadManager(AsyncMode mode)
        {
            Mode = mode;
            cts = new CancellationTokenSource();
            ct = cts.Token;
        }

        /// <summary>
        /// Gets the current <see cref="AsyncMode"/> of this <see cref="ThreadManager"/>.
        /// </summary>
        public AsyncMode Mode { get; }

        /// <summary>
        /// Assigns this ThreadManager to a specific VSLSocket.
        /// </summary>
        /// <param name="parent"></param>
        internal abstract void Assign(VSLSocket parent);

        /// <summary>
        /// Starts an assigned ThreadManager.
        /// </summary>
        internal abstract void Start();
        
        /// <summary>
        /// Invokes an <see cref="Action"/> synchronously.
        /// </summary>
        /// <param name="callback">Work to execute. The <see cref="CancellationToken"/> is going to be canceled when VSL is shutting down.</param>
        /// <exception cref="ArgumentNullException"/>
        public abstract void Invoke(Action<CancellationToken> callback);

        /// <summary>
        /// Invokes an <see cref="Action"/> asynchronously and return a <see cref="Task"/> to await this operation.
        /// </summary>
        /// <param name="callback">Work to execute. The <see cref="CancellationToken"/> is going to be canceled when VSL is shutting down.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <returns></returns>
        public abstract Task InvokeAsync(Action<CancellationToken> callback);

        /// <summary>
        /// Queues an <see cref="Action"/> to be executed asynchronously without watching it.
        /// </summary>
        /// <param name="callback">Work to execute. The <see cref="CancellationToken"/> is going to be canceled when VSL is shutting down.</param>
        /// <exception cref="ArgumentNullException"/>
        public abstract void QueueWorkItem(Action<CancellationToken> callback);

        internal class WorkItem
        {
            internal WorkItem(Action<CancellationToken> work, ManualResetEventSlim waitHandle)
            {
                Work = work;
                WaitHandle = waitHandle;
            }

            internal Action<CancellationToken> Work { get; }
            internal ManualResetEventSlim WaitHandle { get; }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Specify whether managed objects should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // -TODO: dispose managed state (managed objects).
                    cts.Dispose();
                }

                // -TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // -TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // -TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ThreadManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // -TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}