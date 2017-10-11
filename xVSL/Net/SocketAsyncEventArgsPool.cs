using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace VSL
{
    internal sealed class SocketAsyncEventArgsPool
    {
        private ConcurrentBag<SocketAsyncEventArgs> bag;
        internal SocketAsyncEventArgsPool()
        {
            bag = new ConcurrentBag<SocketAsyncEventArgs>();
        }
    }
}