using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace VSL.Threading
{
    internal sealed class ThreadManagerMangedThread : ThreadManager
    {
        private Dispatcher dispatcher;

        internal ThreadManagerMangedThread() : base(AsyncMode.ManagedThread)
        {

        }

        internal ThreadManagerMangedThread(Dispatcher dispatcher) : base(AsyncMode.ManagedThread)
        {
            this.dispatcher = dispatcher;
        }

        internal override void Assign(VSLSocket parent)
        {
            this.parent = parent;
        }

        internal override void Start()
        {
            if (dispatcher == null)
                dispatcher = Dispatcher.CurrentDispatcher;
        }

        internal override void Close()
        {

        }

        public override void Invoke(Action<CancellationToken> callback)
        {
            dispatcher.Invoke(() => callback(itemCt));
        }

        public override async Task InvokeAsync(Action<CancellationToken> callback)
        {
            await dispatcher.InvokeAsync(() => callback(itemCt));
        }

        public override void QueueWorkItem(Action<CancellationToken> callback)
        {
            ThreadPool.QueueUserWorkItem((o) => dispatcher.Invoke(() => callback(itemCt)));
        }
    }
}
