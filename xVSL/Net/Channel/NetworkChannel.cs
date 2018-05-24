using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSL.Net.Channel
{
    internal class NetworkChannel
    {
        private Socket socket;

        private byte[] receiveBuffer;
        private int receiveOffset;
        private int receiveCount;

        private ConcurrentQueue<ReceiveSendItem> realtimeQueue;
        private ConcurrentQueue<ReceiveSendItem> backgroundQueue;
        private readonly object sendLock;
        private bool sending = false;

        public NetworkChannel(Socket socket)
        {
            this.socket = socket ?? throw new ArgumentNullException(nameof(socket));

            receiveBuffer = new byte[0];
            receiveOffset = 0;
            receiveCount = 0;

            realtimeQueue = new ConcurrentQueue<ReceiveSendItem>();
            backgroundQueue = new ConcurrentQueue<ReceiveSendItem>();
            sendLock = new object();
        }

        public Task<bool> ReceiveAsync(byte[] buffer, int offset, int count)
        {
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
            ReceiveSendItem item = new ReceiveSendItem(buffer, offset, count);
            realtimeQueue.Enqueue(item);
            EnsureSend();
            return item.Task;
        }

        public Task<bool> SendAsyncBackground(byte[] buffer, int offset, int count)
        {
            ReceiveSendItem item = new ReceiveSendItem(buffer, offset, count);
            backgroundQueue.Enqueue(item);
            EnsureSend();
            return item.Task;
        }

        private void ReceiveItem(ReceiveSendItem item)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            // TODO: Set buffer
            args.Completed += Send_Completed;
            args.UserToken = item;
            if (!socket.SendAsync(args))
                Send_Completed(socket, args);
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            ReceiveSendItem item = (ReceiveSendItem)e.UserToken;
            if (e.SocketError != SocketError.Success)
            {
                // TODO: Handle bad socket
            }
            if (e.BytesTransferred == 0)
            {
                // TODO: Handle disconnect
            }
            int cplen = Math.Min(e.BytesTransferred, item.Count);
            Array.Copy(e.Buffer, e.Offset, item.Buffer, item.Offset, cplen);
            item.Offset += cplen;
            item.Count -= cplen;
            if (item.Count > 0)
            {
                ReceiveItem(item);
            }
            else
            {
                receiveBuffer = e.Buffer;
                receiveOffset = e.Offset + cplen;
                receiveCount = e.BytesTransferred - cplen;
                item.Tcs.SetResult(true);
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
                sending = true;
                StartSend();
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

        private void StartSend()
        {
            Monitor.Enter(sendLock);
            if (!TryDeqeue(out ReceiveSendItem item))
            {
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
            // TODO: Set buffer
            args.Completed += Send_Completed;
            args.UserToken = item;
            if (!socket.SendAsync(args))
                Send_Completed(socket, args);
        }

        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            ReceiveSendItem item = (ReceiveSendItem)e.UserToken;
            if (e.SocketError != SocketError.Success)
            {
                // TODO: Handle bad socket
            }
            if (e.BytesTransferred == 0)
            {
                // TODO: Handle disconnect
            }
            item.Offset += e.BytesTransferred;
            item.Count -= e.BytesTransferred;
            if (item.Count > 0)
            {
                SendItem(item);
            }
            else
            {
                item.Tcs.SetResult(true);
                StartSend();
            }
        }
    }
}
