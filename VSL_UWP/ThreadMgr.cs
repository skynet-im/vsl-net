using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace VSL
{
    /// <summary>
    /// Manages the event invocation and load balancing of VSL.
    /// </summary>
    public class ThreadMgr : IDisposable
    {
        private VSLSocket parent;
        /// <summary>
        /// Gets the mode how actions are executed.
        /// </summary>
        public InvokeMode Mode { get; }
        /// <summary>
        /// Gets whether the thread manager is started and ready for work.
        /// </summary>
        public bool Started { get; private set; }
        private Task thread;
        private CoreDispatcher dispatcher;
        private ConcurrentQueue<WorkItem> workQueue;
        private CancellationTokenSource cts;
        private CancellationToken ct;
        private CancellationTokenSource itemCts;
        private CancellationToken itemCt;
        // <constructor
        internal ThreadMgr(VSLSocket parent, InvokeMode mode)
        {
            this.parent = parent;
            Mode = mode;
            itemCts = new CancellationTokenSource();
            itemCt = itemCts.Token;
            if (Mode == InvokeMode.Dispatcher)
                Init_Dispatcher();
        }
        private void Init_Dispatcher()
        {
            // TODO: Assign dispatcher
            //dispatcher = new CoreDispatcher();
            Started = true;
        }
        private void Init_ManagedThread()
        {
            Started = true;
            workQueue = new ConcurrentQueue<WorkItem>();
            cts = new CancellationTokenSource();
            ct = cts.Token;
            thread = Task.Run(() => ThreadWork());
            thread.Start();
        }
        /// <summary>
        /// Starts the VSL managed thread.
        /// </summary>
        public void Start()
        {
            if (!Started && Mode == InvokeMode.ManagedThread)
                Init_ManagedThread();
        }
        //  constructor>
        /// <summary>
        /// Invokes an <see cref="Action"/> on the associated thread.
        /// </summary>
        /// <param name="work">Action to execute.</param>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="OperationCanceledException"/>
        public void Invoke(Action<CancellationToken> work)
        {
            if (!Started)
                throw new InvalidOperationException("You have to start the thread manager before using it.");
            //if (Mode == InvokeMode.Dispatcher)
                //dispatcher.Invoke(work);
            else
            {
                ManualResetEventSlim handle = new ManualResetEventSlim();
                workQueue.Enqueue(new WorkItem(work, handle));
                try
                {
                    handle.Wait(ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                finally
                {
                    handle.Dispose();
                }
            }
        }
        /// <summary>
        /// Invokes an <see cref="Action"/> on the associated thread. This method is only available with <see cref="InvokeMode.Dispatcher"/>.
        /// </summary>
        /// <param name="work">Action to execute.</param>
        /// <exception cref="InvalidOperationException"/>
        /// <returns></returns>
        public async Task InvokeAsync(Action<CancellationToken> work)
        {
            if (!Started)
                throw new InvalidOperationException("You have to start the thread manager before using it.");
            if (Mode == InvokeMode.ManagedThread)
                throw new InvalidOperationException("You can not use InvokeAsync(Action work) with InvokeMode.ManagedThread");
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => work.Invoke(itemCt));
        }
        /// <summary>
        /// Queues an <see cref="Action"/> on the associated thread. The <see cref="CancellationToken"/> will be canceled when VSL is shutting down.
        /// </summary>
        /// <param name="work">Action to execute.</param>
        /// <returns>If enqueuing succeeded.</returns>
        public bool QueueWorkItem(Action<CancellationToken> work)
        {
            return QueueItem(work, false);
        }

        /// <summary>
        /// Queues a critical <see cref="Action"/> on the associated thread that will be executed even if VSL is shutting down. The <see cref="CancellationToken"/> will be canceled when VSL is shutting down.
        /// </summary>
        /// <param name="work">Action to execute.</param>
        /// <returns>If enqueuing succeeded.</returns>
        public bool QueueCriticalWorkItem(Action<CancellationToken> work)
        {
            return QueueItem(work, true);
        }

        private bool QueueItem(Action<CancellationToken> work, bool critical)
        {
            if (!Started)
                return false;
            if (Mode == InvokeMode.Dispatcher)
            {
                Task t = InvokeAsync(work);
                return true;
            }
            else if (Mode == InvokeMode.ManagedThread)
            {
                if (!ct.IsCancellationRequested)
                {
                    workQueue.Enqueue(new WorkItem(work, critical));
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Sets the thread to invoke events on to the calling thread. This method is only available with <see cref="InvokeMode.Dispatcher"/>.
        /// </summary>
        public void SetInvokeThread()
        {
            if (Mode == InvokeMode.ManagedThread) throw new InvalidOperationException("You can not use SetInvokeThread() with InvokeMode.ManagedThread");
            // TODO: Assign Dispatcher
            //dispatcher = Dispatcher.CurrentDispatcher;
            Started = true;
        }
        private void ThreadWork()
        {
            try
            {
                lock (disposeLock)
                    readyToDispose = false;
                while (true)
                {
                    while (workQueue.TryDequeue(out WorkItem item))
                    {
                        if (ct.IsCancellationRequested)
                        {
                            Cleanup(item);
                            break;
                        }
                        item.Work.Invoke(itemCt);
                        item.Handle?.Set();
                    }
                    if (ct.WaitHandle.WaitOne(parent.SleepTime))
                    {
                        Cleanup();
                        break;
                    }
                }
                lock (disposeLock)
                {
                    if (disposePending)
                        Dispose();
                    readyToDispose = true;
                }
            }
            catch (Exception ex)
            {
                parent.ExceptionHandler.CloseUncaught(ex);
            }
        }
        private void Cleanup(WorkItem pending = null)
        {
            if (pending != null && pending.Critical)
            {
                pending.Work.Invoke(itemCt);
                pending.Handle?.Set();
            }
            while (workQueue.TryDequeue(out WorkItem item))
            {
                if (item.Critical)
                {
                    item.Work.Invoke(itemCt);
                    item.Handle?.Set();
                }
            }
        }

        /// <summary>
        /// Informs all queued work items that VSL is shutting down.
        /// </summary>
        internal void ShuttingDown()
        {
            itemCts.Cancel();
        }

        internal void Exit()
        {
            cts?.Cancel();
        }
        /// <summary>
        /// The mode how actions are executed.
        /// </summary>
        public enum InvokeMode
        {
            /// <summary>
            /// Actions are invoked on the specified thread using the <see cref="System.Windows.Threading.Dispatcher"/> class. This mode is recommended for UI applications.
            /// </summary>
            Dispatcher,
            /// <summary>
            /// Actions are invoked on a thread, managed by VSL. This mode is recommended for server applications.
            /// </summary>
            ManagedThread
        }
        private class WorkItem
        {
            internal Action<CancellationToken> Work { get; }
            internal ManualResetEventSlim Handle { get; }
            internal bool Critical { get; }
            internal WorkItem(Action<CancellationToken> work, ManualResetEventSlim handle)
                : this(work, handle, false) { }
            internal WorkItem(Action<CancellationToken> work, bool critical)
                : this(work, null, critical) { }
            internal WorkItem(Action<CancellationToken> work, ManualResetEventSlim handle, bool critical)
            {
                Work = work;
                Handle = handle;
                Critical = critical;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private object disposeLock = new object();
        private bool readyToDispose = true;
        private bool disposePending = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Specify whether managed objects should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                lock (disposeLock)
                {
                    if (!readyToDispose)
                    {
                        disposePending = true;
                        return;
                    }
                }

                if (disposing)
                {
                    // -TODO: dispose managed state (managed objects).
                    cts?.Dispose();
                    itemCts?.Cancel();
                }

                // -TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // -TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // -TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ThreadMgr() {
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