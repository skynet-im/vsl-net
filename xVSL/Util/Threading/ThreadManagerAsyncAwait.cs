using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSL.Threading
{
    internal sealed class ThreadManagerAsyncAwait : ThreadManager
    {
        private ConcurrentQueue<WorkItem> queue;
        private object disposeLock;
        private CancellationTokenSource mainCts;
        private CancellationToken mainCt;

        internal ThreadManagerAsyncAwait() : base(AsyncMode.AsyncAwait)
        {
            queue = new ConcurrentQueue<WorkItem>();
            disposeLock = new object();
            mainCts = new CancellationTokenSource();
            mainCt = mainCts.Token;
        }

        internal override void Assign(VSLSocket parent)
        {
            this.parent = parent;
        }

        internal override void Start()
        {
            Start();
        }

        public override void Invoke(Action<CancellationToken> callback)
        {
            using (ManualResetEventSlim handle = new ManualResetEventSlim())
            {
                queue.Enqueue(new WorkItem(callback, handle));
                handle.Wait();
            }
        }

        public override async Task InvokeAsync(Action<CancellationToken> callback)
        {
            using (ManualResetEventSlim handle = new ManualResetEventSlim())
            {
                queue.Enqueue(new WorkItem(callback, handle));
                await Task.Run(() => handle.Wait());
            }
        }

        public override void QueueWorkItem(Action<CancellationToken> callback)
        {
            queue.Enqueue(new WorkItem(callback, null));
        }

        private async void Work()
        {
            try
            {
                while (true)
                {
                    while (queue.TryDequeue(out WorkItem workItem))
                    {
                        workItem.Work(itemCt);
                        workItem.WaitHandle?.Set();
                    }
                    Monitor.Enter(disposeLock);
                    if (mainCt.IsCancellationRequested)
                    {
                        Monitor.Exit(disposeLock);
                        return;
                    }
                    else
                    {
                        Task t = Task.Delay(parent.SleepTime, mainCt);
                        Monitor.Exit(disposeLock);
                        await t;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // shutting down
            }
#if DEBUG
            catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
#else
            catch (Exception ex)
#endif
            {
                parent.ExceptionHandler.CloseUncaught(ex);
            }

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (disposeLock)
                    mainCts.Cancel();
                mainCts.Dispose(); // This will only dispose managed objects.
                //CancellationToken.IsCancellationRequested is still available.
            }
            base.Dispose(disposing);
        }
    }
}