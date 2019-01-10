﻿using System;
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

        private byte[] receiveBuffer;
        private int receiveOffset;
        private int receiveCount;

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

            receiveBuffer = new byte[0];
            receiveOffset = 0;
            receiveCount = 0;

            realtimeQueue = new ConcurrentQueue<ReceiveSendItem>();
            backgroundQueue = new ConcurrentQueue<ReceiveSendItem>();
            sendLock = new object();
        }

        public long ReceivedBytes { get; private set; }
        public long SentBytes { get; private set; }

        public Task<bool> ReceiveAsync(byte[] buffer, int offset, int count)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (shutdown)
                return Task.FromResult(false);

            if (receiveCount > 0)
            {
                int cplen = Math.Min(receiveCount, count);
                Array.Copy(receiveBuffer, receiveOffset, buffer, offset, cplen);
                receiveOffset += cplen;
                receiveCount -= cplen;
                offset += cplen;
                count -= cplen;
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
                DestroyReceive(e);
                return;
            }
            else if (e.SocketError != SocketError.Success)
            {
                item.Tcs.SetResult(false);
                DestroyReceive(e);
                exhandler.CloseConnection(e.SocketError, e.LastOperation);
                return;
            }
            if (e.BytesTransferred == 0)
            {
                item.Tcs.SetResult(false);
                DestroyReceive(e);
                exhandler.CloseConnection(SocketError.Disconnecting, e.LastOperation);
                return;
            }
            ReceivedBytes += e.BytesTransferred;
            int cplen = Math.Min(e.BytesTransferred, item.Count);
            Array.Copy(e.Buffer, e.Offset, item.Buffer, item.Offset, cplen);
            item.Offset += cplen;
            item.Count -= cplen;
            if (item.Count > 0)
            {
                ReceiveItem(item, e);
            }
            else
            {
                receiveBuffer = e.Buffer;
                receiveOffset = e.Offset + cplen;
                receiveCount = e.BytesTransferred - cplen;
                item.Tcs.SetResult(true);
                DestroyReceive(e);
            }
        }

        private void DestroyReceive(SocketAsyncEventArgs args)
        {
            args.Completed -= Receive_Completed;
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
            int cplen = Math.Min(args.Buffer.Length, item.Count);
            Array.Copy(item.Buffer, item.Offset, args.Buffer, 0, cplen);
            args.SetBuffer(0, cplen);
            if (!socket.SendAsync(args))
                Send_Completed(socket, args);
        }

        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            ReceiveSendItem item = (ReceiveSendItem)e.UserToken;
            if (e.SocketError == SocketError.Shutdown)
            {
                item.Tcs.SetResult(false);
                DestroySend(e);
                return;
            }
            else if (e.SocketError != SocketError.Success)
            {
                item.Tcs.SetResult(false);
                exhandler.CloseConnection(e.SocketError, e.LastOperation);
                DestroySend(e);
                return;
            }
            if (e.BytesTransferred == 0)
            {
                item.Tcs.SetResult(false);
                exhandler.CloseConnection(SocketError.ConnectionReset, e.LastOperation);
                DestroySend(e);
                return;
            }
            SentBytes += e.BytesTransferred;
            item.Offset += e.BytesTransferred;
            item.Count -= e.BytesTransferred;
            if (item.Count > 0)
            {
                SendItem(item, e);
            }
            else
            {
                item.Tcs.SetResult(true);
                DestroySend(e);
                StartSend(locked: false);
            }
        }

        private void DestroySend(SocketAsyncEventArgs args)
        {
            args.SetBuffer(0, args.Buffer.Length);
            args.Completed -= Send_Completed;
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
