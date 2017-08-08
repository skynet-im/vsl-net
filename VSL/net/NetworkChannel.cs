using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace VSL
{
    /// <summary>
    /// Responsible for the network communication
    /// </summary>
    internal class NetworkChannel : IDisposable
    {
        // <fields
        internal VSLSocket parent;
        private TcpClient tcp;
        private Queue cache;
        private ConcurrentQueue<byte[]> queue;
        private int _receiveBufferSize = Constants.ReceiveBufferSize;

        private Thread listenerThread;
        private Thread senderThread;
        private Thread workerThread;
        private bool threadsRunning = false;
        private CancellationTokenSource cts;
        private CancellationToken ct;
        //  <stats
        internal long ReceivedBytes;
        internal long SentBytes;
        private ConcurrentQueue<ManualResetEventSlim> verifyingQueue;
        private long probablySentLength;
        //   stats>
        //  fields>

        // <properties
        internal int ReceiveBufferSize
        {
            get
            {
                return _receiveBufferSize;
            }
            set
            {
                _receiveBufferSize = value;
                if (tcp != null) tcp.Client.ReceiveBufferSize = value;
            }
        }
        //  properties>

        // <constructor
        /// <summary>
        /// Initializes a new instance of the NetworkChannel class
        /// </summary>
        /// <param name="parent">Underlying VSL socket</param>
        internal NetworkChannel(VSLSocket parent)
        {
            this.parent = parent;
            InitializeComponent();
        }
        /// <summary>
        /// Initializes a new instance of the NetworkChannel class
        /// </summary>
        /// <param name="parent">Underlying VSL socket</param>
        /// <param name="tcp">Connected TCP client</param>
        internal NetworkChannel(VSLSocket parent, TcpClient tcp)
        {
            this.parent = parent;
            this.tcp = tcp;
            this.tcp.ReceiveBufferSize = _receiveBufferSize;
            InitializeComponent();
            StartTasks();
        }
        /// <summary>
        /// Initializes all non-child-specific components
        /// </summary>
        internal void InitializeComponent()
        {
            cache = new Queue();
            queue = new ConcurrentQueue<byte[]>();
            verifyingQueue = new ConcurrentQueue<ManualResetEventSlim>();
            cts = new CancellationTokenSource();
            ct = cts.Token;
        }
        //  constructor>

        // <functions
        /// <summary>
        /// Sets a VSL socket for the specified client
        /// </summary>
        /// <param name="tcp">TCP listener</param>
        internal void Connect(TcpClient tcp)
        {
            this.tcp = tcp;
            this.tcp.ReceiveBufferSize = _receiveBufferSize;
            StartTasks();
        }

        /// <summary>
        /// Starts the tasks for receiving and compounding
        /// </summary>
        private void StartTasks()
        {
            if (threadsRunning) throw new InvalidOperationException("Tasks are already running.");
            threadsRunning = true;
            listenerThread = new Thread(ListenerThread);
            listenerThread.Start();
            senderThread = new Thread(SenderThread);
            senderThread.Start();
            workerThread = new Thread(SenderThread);
            workerThread.Start();
        }

        /// <summary>
        /// Stops the tasks for receiving and compounding
        /// </summary>
        private void StopTasks()
        {
            threadsRunning = false;
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }

        /// <summary>
        /// Receives bytes from the socket to the cache
        /// </summary>
        /// <returns></returns>
        private void ListenerThread()
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    byte[] buf = new byte[_receiveBufferSize];
                    int len = tcp.Client.Receive(buf, _receiveBufferSize, SocketFlags.None);
                    if (len > 0)
                    {
                        cache.Enqeue(Crypt.Util.TakeBytes(buf, len));
                        ReceivedBytes += len;
                        buf = null;
                    }
                    else
                    {
                        if (ct.WaitHandle.WaitOne(parent.SleepTime))
                            return;
                    }
                    // Socket.Send() does not throw any Exception without connection
                    int cycle = 0;
                    while (verifyingQueue.Count > 0)
                    {
                        if (cycle >= 10 || ct.IsCancellationRequested)
                            break;
                        if (verifyingQueue.TryDequeue(out ManualResetEventSlim handle))
                            handle?.Set();
                        cycle++;
                    }
                }
            }
            catch (SocketException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
        }

        /// <summary>
        /// Compounds packets from the received data
        /// </summary>
        /// <returns></returns>
        private void WorkerThread()
        {
            while (!ct.IsCancellationRequested)
            {
                if (cache.Length > 0)
                {
                    parent.manager.OnDataReceive();
                }
                else
                {
                    if (ct.WaitHandle.WaitOne(parent.SleepTime))
                        return;
                }
            }
        }

        /// <summary>
        /// Sends pending data from the queue
        /// </summary>
        /// <returns></returns>
        private void SenderThread()
        {
            while (!ct.IsCancellationRequested)
            {
                if (queue.Count > 0)
                {
                    byte[] buf = new byte[0];
                    if (queue.TryDequeue(out buf))
                    {
                        Send(buf);
                    }
                    else
                    {
                        parent.Logger.I("Error at dequeuing the send queue in NetworkChannel.SenderTask");
                    }
                }
                else
                {
                    if (ct.WaitHandle.WaitOne(parent.SleepTime))
                        return;
                }
            }
        }

        /// <summary>
        /// Reads data from the buffer.
        /// </summary>
        /// <param name="count">count of bytes to read</param>
        /// <exception cref="OperationCanceledException"/>
        /// <exception cref="TimeoutException"/>
        /// <returns></returns>
        internal byte[] Read(int count)
        {
            const int wait = 10;
            const int cycles = Constants.ReceiveTimeout / wait;
            int cycle = 0;
            while (cache.Length < count)
            {
                if (cycle >= cycles)
                    throw new TimeoutException();
                if (ct.WaitHandle.WaitOne(wait))
                    throw new OperationCanceledException();
                cycle++;
            }
            if (!cache.Dequeue(out byte[] buf, count))
                throw new Exception("Error at dequeueing bytes");
            return buf;
        }
        /// <summary>
        /// Reads data from the buffer asynchronously.
        /// </summary>
        /// <param name="count">Count of bytes to read.</param>
        /// <exception cref="TaskCanceledException"/>
        /// <exception cref="TimeoutException"/>
        /// <returns></returns>
        internal async Task<byte[]> ReadAsync(int count)
        {
            const int wait = 10;
            const int cycles = Constants.ReceiveTimeout / wait;
            int cycle = 0;
            while (cache.Length < count)
            {
                if (cycle >= cycles)
                    throw new TimeoutException();
                await Task.Delay(10, ct);
                cycle++;
            }
            if (!cache.Dequeue(out byte[] buf, count))
                throw new Exception("Error at dequeueing bytes");
            return buf;
        }

        /// <summary>
        /// Sends data to the remote client
        /// </summary>
        /// <param name="buf">data to send</param>
        internal int Send(byte[] buf)
        {
            int sent = 0;
            try
            {
                sent = tcp.Client.Send(buf);
            }
            catch (SocketException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return 0;
            }
            ManualResetEventSlim handle = new ManualResetEventSlim();
            verifyingQueue.Enqueue(handle);
            try
            {
                handle.Wait(ct);
            }
            catch (OperationCanceledException)
            {
                handle.Dispose();
                return 0;
            }
            handle.Dispose();
            SentBytes += sent;
            return sent;
        }

        ///// <summary>
        ///// Sends data to the remote client asynchronously
        ///// </summary>
        ///// <param name="buf">data to send</param>
        //internal void SendAsync(byte[] buf)
        //{
        //    queue.Enqueue(buf);
        //}

        /// <summary>
        /// Sends data to the remote client asynchronously
        /// </summary>
        /// <param name="buf">data to send</param>
        internal async Task<int> SendAsync(byte[] buf)
        {
            ManualResetEventSlim handle = new ManualResetEventSlim();
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.SetBuffer(buf, 0, buf.Length);
            e.Completed += delegate { handle.Set(); };
            tcp.Client.SendAsync(e);
            await Task.Run(new Action(delegate
            {
                try
                {
                    handle.Wait(ct);
                }
                catch (OperationCanceledException)
                {

                }
            }));
            handle.Dispose();
            int sent = e.BytesTransferred;
            SentBytes += sent;
            e.Dispose();
            return sent;
        }

        /// <summary>
        /// Stops the network channel and closes the TCP connection without raising the related event
        /// </summary>
        internal void CloseConnection()
        {
            StopTasks();
            tcp.Close();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // -TODO: dispose managed state (managed objects).
                    cts.Dispose();
                }

                // -TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // -TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // -TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NetworkChannel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // -TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        //  functions>
    }
}