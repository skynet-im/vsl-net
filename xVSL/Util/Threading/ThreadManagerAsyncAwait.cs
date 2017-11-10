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
        private CancellationTokenSource mainCts;
        private CancellationToken mainCt;

        internal ThreadManagerAsyncAwait() : base(AsyncMode.AsyncAwait)
        {
            queue = new ConcurrentQueue<WorkItem>();
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

        internal override void Close()
        {
            mainCts.Cancel();
            Dispose();
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
                        if (mainCt.IsCancellationRequested)
                            return;
                    }
                    await Task.Delay(parent.SleepTime, mainCt);
                }
            }
            catch (TaskCanceledException)
            {
                // shutting down
            }
            catch (Exception ex)
            {
                parent.ExceptionHandler.CloseUncaught(ex);
            }
            
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mainCts.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}