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

        internal override void Assign(VSLSocket parent)
        {
            this.parent = parent;
        }

        internal override void Start()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public override void Invoke(Action<CancellationToken> callback)
        {
            dispatcher.Invoke(() => callback(ct));
        }

        public override async Task InvokeAsync(Action<CancellationToken> callback)
        {
            await dispatcher.InvokeAsync(() => callback(ct));
        }

        public override void QueueWorkItem(Action<CancellationToken> callback)
        {
            dispatcher.InvokeAsync(() => callback(ct));
        }
    }
}
