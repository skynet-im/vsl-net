using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSL.Threading
{
    internal sealed class ThreadManagerMangedThread : ThreadManager
    {
        private NSObject threadAccess;

        internal ThreadManagerMangedThread(NSObject threadAccess) : base(AsyncMode.ManagedThread)
        {
            this.threadAccess = threadAccess;
        }

        internal override void Assign(VSLSocket parent)
        {
            this.parent = parent;
        }

        internal override void Start()
        {
            
        }

        public override void Invoke(Action<CancellationToken> callback)
        {
            threadAccess.InvokeOnMainThread(() => callback(itemCt));
        }

        public override async Task InvokeAsync(Action<CancellationToken> callback)
        {
            await Task.Run(() => threadAccess.InvokeOnMainThread(() => callback(itemCt)));
        }

        public override void QueueWorkItem(Action<CancellationToken> callback)
        {
            ThreadPool.QueueUserWorkItem((o) => threadAccess.InvokeOnMainThread(() => callback(itemCt)));
        }
    }
}
