using System;
using System.Threading;

namespace VSL
{
    /// <summary>
    /// Provides the ability to invoke delegates on the initial <see cref="SynchronizationContext"/>
    /// </summary>
    internal sealed class InvokationManager
    {
        private static readonly Lazy<SynchronizationContext> lazyDefaultContext = new Lazy<SynchronizationContext>();
        private SynchronizationContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokationManager"/> class using the current <see cref="SynchronizationContext"/>.
        /// </summary>
        public InvokationManager()
        {
            SetContext();
        }

        /// <summary>
        /// Dipatches a synchronous message to a synchronization context.
        /// </summary>
        /// <param name="callback"></param>
        public void Send(Action callback)
        {
            context.Send(o => callback(), null);
        }

        /// <summary>
        /// Dispatches an asynchronous message to a synchronization context.
        /// </summary>
        /// <param name="callback"></param>
        public void Post(Action callback)
        {
            context.Post(o => callback(), null);
        }

        /// <summary>
        /// Sets the current <see cref="SynchronizationContext"/> of the calling thread.
        /// </summary>
        public void SetContext()
        {
            SetContext(SynchronizationContext.Current ?? lazyDefaultContext.Value);
        }

        /// <summary>
        /// Sets the specified <see cref="SynchronizationContext"/>.
        /// </summary>
        public void SetContext(SynchronizationContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }
}
