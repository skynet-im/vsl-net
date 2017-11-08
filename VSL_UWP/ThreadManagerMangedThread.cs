using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSL.Threading
{
    internal sealed class ThreadManagerMangedThread : ThreadManager
    {
        internal ThreadManagerMangedThread() : base(AsyncMode.ManagedThread)
        {

        }

        public override void Invoke(Action<CancellationToken> callback)
        {

        }

        public override Task InvokeAsync(Action<CancellationToken> callback)
        {

        }

        public override void QueueWorkItem(Action<CancellationToken> callback)
        {

        }
    }
}
