using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSL.Net.Channel
{
    internal sealed class NetworkChannel : IDisposable
    {
        private Socket socket;
        private ExceptionHandler exception;

        private byte[] receiveBuffer;
        private int receiveOffset;
        private int receiveCount;

        private ConcurrentQueue<ReceiveSendItem> realtimeQueue;
        private ConcurrentQueue<ReceiveSendItem> backgroundQueue;
        private readonly object sendLock;
        private bool sending = false;

        private bool shutdown = false;
        private bool disposed = false;

        public NetworkChannel(Socket socket, ExceptionHandler exception)
        {
            this.socket = socket ?? throw new ArgumentNullException(nameof(socket));
            this.exception = exception ?? throw new ArgumentNullException(nameof(exception));

            receiveBuffer = new byte[0];
            receiveOffset = 0;
            receiveCount = 0;

            realtimeQueue = new ConcurrentQueue<ReceiveSendItem>();
            backgroundQueue = new ConcurrentQueue<ReceiveSendItem>();
            sendLock = new object();
        }

        public Task<bool> ReceiveAsync(byte[] buffer, int offset, int count)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(NetworkChannel));
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
                throw new ObjectDisposedException(nameof(NetworkChannel));
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
                throw new ObjectDisposedException(nameof(NetworkChannel));
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
                throw new ObjectDisposedException(nameof(NetworkChannel));

            if (!shutdown)
            {
                socket.Shutdown(SocketShutdown.Both);
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
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[socket.ReceiveBufferSize], 0, socket.ReceiveBufferSize);
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
            if (e.SocketError == SocketError.Shutdown)
            {
                item.Tcs.SetResult(false);
                e.Dispose();
                return;
            }
            else if (e.SocketError != SocketError.Success)
            {
                item.Tcs.SetResult(false);
                e.Dispose();
                exception.CloseConnection(e.SocketError, e.LastOperation);
                return;
            }
            if (e.BytesTransferred == 0)
            {
                item.Tcs.SetResult(false);
                e.Dispose();
                exception.CloseConnection(SocketError.ConnectionReset, e.LastOperation);
                return;
            }
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
                e.Dispose();
            }
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

        private bool TryDeqeue(out ReceiveSendItem item)
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
            if (!TryDeqeue(out ReceiveSendItem item))
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
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[socket.SendBufferSize], 0, socket.SendBufferSize);
            args.Completed += Send_Completed;
            SendItem(item, args);
        }

        private void SendItem(ReceiveSendItem item, SocketAsyncEventArgs args)
        {
            args.UserToken = item;
            if (!socket.SendAsync(args))
                Send_Completed(socket, args);
        }

        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            ReceiveSendItem item = (ReceiveSendItem)e.UserToken;
            if (e.SocketError == SocketError.Shutdown)
            {
                item.Tcs.SetResult(false);
                e.Dispose();
                return;
            }
            else if (e.SocketError != SocketError.Success)
            {
                item.Tcs.SetResult(false);
                exception.CloseConnection(e.SocketError, e.LastOperation);
                e.Dispose();
                return;
            }
            if (e.BytesTransferred == 0)
            {
                item.Tcs.SetResult(false);
                exception.CloseConnection(SocketError.ConnectionReset, e.LastOperation);
                e.Dispose();
                return;
            }
            item.Offset += e.BytesTransferred;
            item.Count -= e.BytesTransferred;
            if (item.Count > 0)
            {
                SendItem(item, e);
            }
            else
            {
                item.Tcs.SetResult(true);
                e.Dispose();
                StartSend(locked: false);
            }
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
