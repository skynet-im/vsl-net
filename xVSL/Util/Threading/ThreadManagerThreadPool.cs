using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSL.Threading
{
    internal sealed class ThreadManagerThreadPool : ThreadManager
    {
        private ConcurrentQueue<WorkItem> queue; 
        private Timer timer;
        private CancellationTokenSource mainCts;
        private CancellationToken mainCt;

        internal ThreadManagerThreadPool() : base(AsyncMode.ThreadPool)
        {
            queue = new ConcurrentQueue<WorkItem>();
            timer = new Timer(TimerCallback, null, -1, -1);
            mainCts = new CancellationTokenSource();
            mainCt = mainCts.Token;
        }

        internal override void Assign(VSLSocket parent)
        {
            this.parent = parent;
        }

        internal override void Start()
        {
            timer.Change(0, -1);
        }

        internal override void Close()
        {
            mainCts.Cancel();
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

        private void TimerCallback(object state)
        {
            if (mainCt.IsCancellationRequested)
                return;
            while (queue.TryDequeue(out WorkItem workItem))
            {
                workItem.Work(itemCt);
                workItem.WaitHandle?.Set();
            }
            timer.Change(parent.SleepTime, -1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
                mainCts.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}