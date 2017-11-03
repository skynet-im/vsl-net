using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSL.Threading
{
    internal class ThreadManagerThreadPool : ThreadManager
    {
        private ConcurrentQueue<Action<CancellationToken>> queue; 
        private Timer timer;

        internal ThreadManagerThreadPool(VSLSocket parent) : base(parent, AsyncMode.ThreadPool)
        {
            queue = new ConcurrentQueue<Action<CancellationToken>>();
            timer = new Timer(TimerCallback, cts.Token, 0, -1);
        }

        public override void Invoke(Action<CancellationToken> callback)
        {
            queue.Enqueue(callback);
            // TODO: Wait for completition
        }

        public override Task InvokeAsync(Action<CancellationToken> callback)
        {
            queue.Enqueue(callback);
            // TODO: Wait for completition
            return null;
        }

        public override void QueueWorkItem(Action<CancellationToken> callback)
        {
            queue.Enqueue(callback);
        }

        private void TimerCallback(object state)
        {
            while (queue.TryDequeue(out Action<CancellationToken> action))
            {
                action((CancellationToken)state);
            }
            timer.Change(parent.SleepTime, -1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
