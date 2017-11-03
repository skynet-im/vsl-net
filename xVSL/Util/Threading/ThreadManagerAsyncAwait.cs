using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSL.Threading
{
    internal class ThreadManagerAsyncAwait : ThreadManager
    {
        internal ThreadManagerAsyncAwait(VSLSocket parent) : base(parent, AsyncMode.AsyncAwait)
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
