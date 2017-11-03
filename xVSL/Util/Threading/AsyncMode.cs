using System;
using System.Collections.Generic;
using System.Text;

namespace VSL
{
    /// <summary>
    /// Specifies how work is executed by the <see cref="ThreadManager"/>.
    /// </summary>
    public enum AsyncMode
    {
        /// <summary>
        /// Actions are executed in the correct order on the thread pool using the <see cref="System.Threading.Timer"/> class.
        /// </summary>
        ThreadPool,
        /// <summary>
        /// Actions are executed on a managed thread. In most cases this will be the UI-Thread.
        /// </summary>
        ManagedThread,
        /// <summary>
        /// Actions are executed on the main thread and VSL uses async/await calls internally.
        /// </summary>
        AsyncAwait
    }
}