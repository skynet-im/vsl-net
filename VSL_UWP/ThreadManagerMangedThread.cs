﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;

namespace VSL.Threading
{
    internal sealed class ThreadManagerMangedThread : ThreadManager
    {
        CoreWindow window;
        CoreDispatcher dispatcher;

        internal ThreadManagerMangedThread() : base(AsyncMode.ManagedThread)
        {

        }

        internal ThreadManagerMangedThread(CoreWindow window) : base(AsyncMode.ManagedThread)
        {
            this.window = window;
        }

        internal override void Assign(VSLSocket parent)
        {
            this.parent = parent;
        }

        internal override void Start()
        {
            if (window == null)
            {
                window = CoreWindow.GetForCurrentThread();
                window.Activate();
            }
            dispatcher = window.Dispatcher;
        }

        public override void Invoke(Action<CancellationToken> callback)
        {
            dispatcher.RunAsync(CoreDispatcherPriority.Normal, Work(callback)).AsTask().Wait();
        }

        public override Task InvokeAsync(Action<CancellationToken> callback)
        {
            return dispatcher.RunAsync(CoreDispatcherPriority.Normal, Work(callback)).AsTask();
        }

        public override async void QueueWorkItem(Action<CancellationToken> callback)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, Work(callback));
        }

        private DispatchedHandler Work(Action<CancellationToken> callback) => delegate
        {
            try
            {
                callback(itemCt);
            }
            catch (Exception ex) when (parent.CatchApplicationExceptions)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
        };
    }
}
