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

        internal ThreadManagerAsyncAwait() : base(AsyncMode.AsyncAwait)
        {
            queue = new ConcurrentQueue<WorkItem>();
            Work();
        }

        internal override void Assign(VSLSocket parent)
        {
            this.parent = parent;
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
            while (true)
            {
                while (queue.TryDequeue(out WorkItem workItem))
                {
                    workItem.Work(ct);
                    workItem.WaitHandle?.Set();
                }
                await Task.Delay(parent.SleepTime);
            }
        }
    }
}
