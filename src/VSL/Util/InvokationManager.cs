using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VSL
{
    /// <summary>
    /// Provides the ability to invoke delegates on the initial <see cref="SynchronizationContext"/>
    /// </summary>
    internal class InvokationManager
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

        public void Send(Action callback)
        {
            context.Send(o => callback(), null);
        }

        public void Post(Action callback)
        {
            context.Post(o => callback(), null);
        }

        /// <summary>
        /// Sets the current <see cref="SynchronizationContext"/> of the calling thead.
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
