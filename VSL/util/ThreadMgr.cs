using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace VSL
{
    /// <summary>
    /// Manages the event invocation and load balancing of VSL.
    /// </summary>
    public class ThreadMgr
    {
        private VSLSocket parent;
        /// <summary>
        /// Gets the mode how actions are executed.
        /// </summary>
        public InvokeMode Mode { get; }
        private Thread thread;
        private Dispatcher dispatcher;
        private ConcurrentQueue<WorkItem> workQueue;
        private CancellationTokenSource cts;
        private CancellationToken ct;
        // <constructor
        internal ThreadMgr(VSLSocket parent, InvokeMode mode)
        {
            this.parent = parent;
            Mode = mode;
            if (mode == InvokeMode.Dispatcher)
                Init_Dispatcher();
            else if (mode == InvokeMode.ManagedThread)
                Init_ManagedThread();
        }
        private void Init_Dispatcher()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }
        private void Init_ManagedThread()
        {
            workQueue = new ConcurrentQueue<WorkItem>();
            cts = new CancellationTokenSource();
            ct = cts.Token;
            thread = new Thread(ThreadWork);
            thread.Start();
        }
        //  constructor>
        /// <summary>
        /// Invokes an <see cref="Action"/> on the associated thread.
        /// </summary>
        /// <param name="work">Action to execute.</param>
        /// <exception cref="OperationCanceledException"/>
        public void Invoke(Action work)
        {
            if (Mode == InvokeMode.Dispatcher)
                dispatcher.Invoke(work);
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
        /// <returns></returns>
        public async Task InvokeAsync(Action work)
        {
            if (Mode == InvokeMode.ManagedThread) throw new InvalidOperationException("You can not use InvokeAsync(Action work) with InvokeMode.ManagedThread");
            await dispatcher.InvokeAsync(work);
        }
        /// <summary>
        /// Queues an Action on the associated thread.
        /// </summary>
        /// <param name="work">Action to execute.</param>
        /// <param name="critical">Critical item will be executed if enqueuing succeeded.</param>
        /// <returns>If the enqueuing succeeded.</returns>
        public bool QueueWorkItem(Action work, bool critical = false)
        {
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
            dispatcher = Dispatcher.CurrentDispatcher;
        }
        /// <summary>
        /// Sets the specified thread as thread to invoke events on. This method is only available with <see cref="InvokeMode.Dispatcher"/>.
        /// </summary>
        /// <param name="invokeThread">Thread to invoke events on.</param>
        public void SetInvokeThread(Thread invokeThread)
        {
            if (Mode == InvokeMode.ManagedThread) throw new InvalidOperationException("You can not use SetInvokeThread(Thread invokeThread) with InvokeMode.ManagedThread");
            dispatcher = Dispatcher.FromThread(invokeThread);
        }
        private void ThreadWork()
        {
            while (true)
            {
                while (workQueue.TryDequeue(out WorkItem item))
                {
                    if (ct.IsCancellationRequested)
                    {
                        Cleanup(item);
                        return;
                    }
                    item.Work.Invoke();
                    item.Handle?.Set();
                }
                if (ct.WaitHandle.WaitOne(parent.SleepTime))
                {
                    Cleanup();
                    return;
                }
            }
        }
        private void Cleanup(WorkItem pending = null)
        {
            if (pending != null && pending.Critical)
            {
                pending.Work.Invoke();
                pending.Handle?.Set();
            }
            while (workQueue.TryDequeue(out WorkItem item))
            {
                if (item.Critical)
                {
                    item.Work.Invoke();
                    item.Handle?.Set();
                }
            }
        }

        internal void Exit()
        {
            cts.Cancel();
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
            internal Action Work { get; }
            internal ManualResetEventSlim Handle { get; }
            internal bool Critical { get; }
            internal WorkItem(Action work, ManualResetEventSlim handle)
                : this(work, handle, false) { }
            internal WorkItem(Action work, bool critical)
                : this(work, null, critical) { }
            internal WorkItem(Action work, ManualResetEventSlim handle, bool critical)
            {
                Work = work;
                Handle = handle;
                Critical = critical;
            }
        }
    }
}