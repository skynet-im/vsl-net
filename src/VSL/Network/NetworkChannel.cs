using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace VSL.Network
{
    internal sealed class NetworkChannel : IDisposable
    {
        private readonly Socket socket;
        private readonly ExceptionHandler exhandler;
        private readonly MemoryCache<SocketAsyncEventArgs> cache;

        private SocketAsyncEventArgs receiveCache;

        private readonly ConcurrentQueue<ReceiveSendItem> realtimeQueue;
        private readonly ConcurrentQueue<ReceiveSendItem> backgroundQueue;
        private readonly object sendLock;
        private bool sending = false;

        private bool shutdown = false;
        private bool disposed = false;

        public NetworkChannel(Socket socket, ExceptionHandler exhandler, MemoryCache<SocketAsyncEventArgs> cache)
        {
            this.socket = socket ?? throw new ArgumentNullException(nameof(socket));
            this.exhandler = exhandler ?? throw new ArgumentNullException(nameof(exhandler));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));

            realtimeQueue = new ConcurrentQueue<ReceiveSendItem>();
            backgroundQueue = new ConcurrentQueue<ReceiveSendItem>();
            sendLock = new object();
        }

        public long ReceivedBytes { get; private set; }
        public long SentBytes { get; private set; }

        /// <summary>
        /// Receives the specified count of bytes from the remote host asynchronously. This method is not threadsafe!
        /// </summary>
        /// <exception cref="ObjectDisposedException" />
        public Task<bool> ReceiveAsync(byte[] buffer, int offset, int count)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (shutdown)
                return Task.FromResult(false);

            if (receiveCache != null)
            {
                int cplen = Math.Min(receiveCache.Count, count);
                Array.Copy(receiveCache.Buffer, receiveCache.Offset, buffer, offset, cplen);
                receiveCache.SetBuffer(receiveCache.Offset + cplen, receiveCache.Count - cplen);
                offset += cplen;
                count -= cplen;

                if (receiveCache.Count == 0)
                {
                    RecycleReceive(receiveCache);
                    receiveCache = null;
                }
            }
            if (count > 0)
            {
                ReceiveSendItem item = new ReceiveSendItem(buffer, offset, count);
                ReceiveItem(item);
                return item.Task;
            }
            else
            {
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Sends data to the remote host asynchronously.
        /// </summary>
        /// <exception cref="ObjectDisposedException" />
        public Task<bool> SendAsync(byte[] buffer, int offset, int count)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (shutdown)
                return Task.FromResult(false);

            ReceiveSendItem item = new ReceiveSendItem(buffer, offset, count);
            realtimeQueue.Enqueue(item);
            EnsureSend();
            return item.Task;
        }

        /// <summary>
        /// Sends data to the remote host asynchronously without blocking the connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException" />
        public Task<bool> SendAsyncBackground(byte[] buffer, int offset, int count)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (shutdown)
                return Task.FromResult(false);

            ReceiveSendItem item = new ReceiveSendItem(buffer, offset, count);
            backgroundQueue.Enqueue(item);
            EnsureSend();
            return item.Task;
        }

        public void Shutdown()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (!shutdown)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException) { }
                // Errors may occur when shutting down while the counterpart has closed the connection.

                while (realtimeQueue.TryDequeue(out ReceiveSendItem item))
                    item.Tcs.SetResult(false);
                while (backgroundQueue.TryDequeue(out ReceiveSendItem item))
                    item.Tcs.SetResult(false);
                shutdown = true;
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                socket.Dispose();
                if (receiveCache != null)
                {
                    RecycleReceive(receiveCache);
                    receiveCache = null;
                }

                disposed = true;
            }
        }

        private void ReceiveItem(ReceiveSendItem item)
        {
            SocketAsyncEventArgs args = cache.PopOrCreate();
            args.Completed += Receive_Completed;
            ReceiveItem(item, args);
        }

        private void ReceiveItem(ReceiveSendItem item, SocketAsyncEventArgs args)
        {
            args.UserToken = item;
            if (!socket.ReceiveAsync(args))
                Receive_Completed(socket, args);
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            ReceiveSendItem item = (ReceiveSendItem)e.UserToken;
            if (e.SocketError == SocketError.Shutdown || e.SocketError == SocketError.OperationAborted)
            {
                item.Tcs.SetResult(false);
                RecycleReceive(e);
                return;
            }
            else if (e.SocketError != SocketError.Success)
            {
                item.Tcs.SetResult(false);
                RecycleReceive(e);
                exhandler.CloseConnection(e.SocketError, e.LastOperation);
                return;
            }
            if (e.BytesTransferred == 0)
            {
                item.Tcs.SetResult(false);
                RecycleReceive(e);
                exhandler.CloseConnection(SocketError.Disconnecting, e.LastOperation);
                return;
            }
            ReceivedBytes += e.BytesTransferred; // for statistics
            int cplen = Math.Min(e.BytesTransferred, item.Count);
            Array.Copy(e.Buffer, e.Offset, item.Buffer, item.Offset, cplen);
            item.Offset += cplen;
            item.Count -= cplen;
            if (item.Count > 0)
            {
                ReceiveItem(item, e);
            }
            else if (e.BytesTransferred - cplen > 0)
            {
                e.SetBuffer(e.Offset + cplen, e.BytesTransferred - cplen);
                receiveCache = e;
                item.Tcs.SetResult(true);
                // Do not recycle this item now when we cache its buffer
            }
            else
            {
                item.Tcs.SetResult(true);
                RecycleReceive(e);
            }
        }

        private void RecycleReceive(SocketAsyncEventArgs args)
        {
            args.SetBuffer(0, args.Buffer.Length);
            args.Completed -= Receive_Completed;
            args.UserToken = null;
            cache.Push(args);
        }

        /// <summary>
        /// Ensures that a message send callback loop is running. If none is running it starts one.
        /// </summary>
        private void EnsureSend()
        {
            Monitor.Enter(sendLock);
            if (sending)
            {
                Monitor.Exit(sendLock);
            }
            else
            {
                StartSend(locked: true);
            }
        }

        private bool TryDequeue(out ReceiveSendItem item)
        {
            if (realtimeQueue.TryDequeue(out item))
                return true;
            if (backgroundQueue.TryDequeue(out item))
                return true;
            return false;
        }

        private void StartSend(bool locked)
        {
            if (!locked)
                Monitor.Enter(sendLock);
            if (TryDequeue(out ReceiveSendItem item))
            {
                sending = true;
                Monitor.Exit(sendLock);
                SendItem(item);
            }
            else
            {
                sending = false;
                Monitor.Exit(sendLock);
            }
        }

        private void SendItem(ReceiveSendItem item)
        {
            SocketAsyncEventArgs args = cache.PopOrCreate();
            args.Completed += Send_Completed;
            SendItem(item, args);
        }

        private void SendItem(ReceiveSendItem item, SocketAsyncEventArgs args)
        {
            args.UserToken = item;
            int cplen = Math.Min(args.Count, item.Count);
            Array.Copy(item.Buffer, item.Offset, args.Buffer, args.Offset, cplen);
            args.SetBuffer(args.Offset, cplen);
            if (!socket.SendAsync(args))
                Send_Completed(socket, args);
        }

        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            ReceiveSendItem item = (ReceiveSendItem)e.UserToken;
            if (e.SocketError == SocketError.Shutdown)
            {
                item.Tcs.SetResult(false);
                RecycleSend(e);
                return;
            }
            else if (e.SocketError != SocketError.Success)
            {
                item.Tcs.SetResult(false);
                exhandler.CloseConnection(e.SocketError, e.LastOperation);
                RecycleSend(e);
                return;
            }
            if (e.BytesTransferred == 0)
            {
                item.Tcs.SetResult(false);
                exhandler.CloseConnection(SocketError.ConnectionReset, e.LastOperation);
                RecycleSend(e);
                return;
            }
            SentBytes += e.BytesTransferred; // for statistics
            item.Offset += e.BytesTransferred;
            item.Count -= e.BytesTransferred;
            if (item.Count > 0)
            {
                SendItem(item, e);
            }
            else
            {
                item.Tcs.SetResult(true);
                RecycleSend(e);
                StartSend(locked: false);
            }
        }

        private void RecycleSend(SocketAsyncEventArgs args)
        {
            args.SetBuffer(0, args.Buffer.Length);
            args.Completed -= Send_Completed;
            args.UserToken = null;
            cache.Push(args);
        }
    }

    internal struct ReceiveSendItem
    {
        internal ReceiveSendItem(byte[] buffer, int offset, int count)
        {
            Tcs = new TaskCompletionSource<bool>();
            Task = Tcs.Task;
            Buffer = buffer;
            Offset = offset;
            Count = count;
        }

        internal TaskCompletionSource<bool> Tcs { get; }
        internal Task<bool> Task { get; }
        internal byte[] Buffer { get; }
        internal int Offset { get; set; }
        internal int Count { get; set; }
    }
}
