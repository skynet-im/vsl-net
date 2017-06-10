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
        internal VSLClient parent;
        private TcpClient tcp;
        private Queue cache;
        private ConcurrentQueue<byte[]> queue;
        private int _networkBufferSize = Constants.ReceiveBufferSize;

        private bool threadsRunning = false;
        private CancellationTokenSource cts;
        private CancellationToken ct;
        //  fields>

        // <properties
        internal int NetworkBufferSize
        {
            get
            {
                return _networkBufferSize;
            }
            set
            {
                _networkBufferSize = value;
                if (tcp != null) tcp.Client.ReceiveBufferSize = value;
            }
        }
        //  properties>

        // <constructor
        /// <summary>
        /// Initializes a new instance of the NetworkChannel class
        /// </summary>
        /// <param name="parent">Underlying VSL socket</param>
        internal NetworkChannel(VSLClient parent)
        {
            this.parent = parent;
            InitializeComponent();
        }
        /// <summary>
        /// Initializes a new instance of the NetworkChannel class
        /// </summary>
        /// <param name="parent">Underlying VSL socket</param>
        /// <param name="tcp">Connected TCP client</param>
        internal NetworkChannel(VSLClient parent, TcpClient tcp)
        {
            this.parent = parent;
            this.tcp = tcp;
            this.tcp.ReceiveBufferSize = _networkBufferSize;
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
            this.tcp.ReceiveBufferSize = _networkBufferSize;
            StartTasks();
        }

        /// <summary>
        /// Starts the tasks for receiving and compounding
        /// </summary>
        private void StartTasks()
        {
            if (threadsRunning) throw new InvalidOperationException("Tasks are already running.");
            threadsRunning = true;
            Task listenerTask = ListenerTask();
            Task workerTask = WorkerTask();
            Task senderTask = SenderTask();
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
        private async Task ListenerTask()
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    byte[] buf = new byte[_networkBufferSize];
                    int len = await ReceiveAsync(buf, _networkBufferSize, SocketFlags.None);
                    if (len == 0)
                    {
                        await Task.Delay(10, ct);
                    }
                    cache.Enqeue(buf.Take(len).ToArray());
                    buf = null;
                }
            }
            catch (SocketException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
        //<#Xamarin
        private async Task<int> ReceiveAsync(byte[] buffer, int size, SocketFlags socketFlags)
        {
            ReceiveResult r = await Task.Run(() => ReceiveBlocking(size, socketFlags));
            if (r.Buffer == null)
                throw r.Exception;
            buffer = r.Buffer;
            return r.Length;
        }

        private ReceiveResult ReceiveBlocking(int size, SocketFlags socketFlags)
        {
            try
            {
                byte[] buf = new byte[size];
                int length = tcp.Client.Receive(buf, size, socketFlags);
                return new ReceiveResult(length, buf, null);
            }
            catch (Exception ex)
            {
                return new ReceiveResult(0, null, ex);
            }
        }

        private struct ReceiveResult
        {
            internal int Length;
            internal byte[] Buffer;
            internal Exception Exception;
            internal ReceiveResult(int length, byte[] buffer, Exception ex)
            {
                Length = length;
                Buffer = buffer;
                Exception = ex;
            }
        }
        // #Xamarin>

        /// <summary>
        /// Compounds packets from the received data
        /// </summary>
        /// <returns></returns>
        private async Task WorkerTask()
        {
            while (threadsRunning)
            {
                if (cache.Length > 0)
                {
                    await parent.manager.OnDataReceiveAsync();
                }
                else
                {
                    await Task.Delay(10, ct);
                }
            }
        }

        /// <summary>
        /// Sends pending data from the queue
        /// </summary>
        /// <returns></returns>
        private async Task SenderTask()
        {
            while (!ct.IsCancellationRequested)
            {
                if (queue.Count > 0)
                {
                    byte[] buf = new byte[0];
                    if (queue.TryDequeue(out buf))
                    {
                        await Task.Run(() => SendRaw(buf));
                    }
                    else
                    {
                        parent.Logger.i("[VSL] Error at dequeuing the send queue in NetworkChannel.SenderTask");
                    }
                }
                else
                {
                    try
                    {
                        await Task.Delay(10, ct);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                    if (!threadsRunning)
                        return;
                }
            }
        }

        /// <summary>
        /// Reads data from the buffer
        /// </summary>
        /// <param name="count">count of bytes to read</param>
        /// <returns></returns>
        internal async Task<byte[]> ReadAsync(int count)
        {
            int cycle = 1;
            while (cache.Length < count)
            {
                if (cycle >= 1000) throw new TimeoutException();
                await Task.Delay(50, ct);
                cycle++;
            }
            byte[] buf = new byte[count];
            bool success = cache.Dequeue(out buf, count);
            if (!success)
                throw new Exception("Error in the cache");
            return buf;
        }

        /// <summary>
        /// Sends data to the remote client
        /// </summary>
        /// <param name="buf">data to send</param>
        internal void SendRaw(byte[] buf)
        {
            try
            {
                tcp.Client.Send(buf);
                buf = null;
            }
            catch (SocketException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
            }
        }

        /// <summary>
        /// Sends data to the remote client asynchronously
        /// </summary>
        /// <param name="buf">data to send</param>
        internal void SendAsync(byte[] buf)
        {
            queue.Enqueue(buf);
        }

        /// <summary>
        /// Stops the network channel and closes the TCP connection without raising the related event
        /// </summary>
        internal void CloseConnection()
        {
            StopTasks();
            tcp.Dispose();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    cts.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NetworkChannel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        //  functions>
    }
}