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
        private VSLSocket parent;
        private TcpClient tcp;
        private Queue cache;
        private int _receiveBufferSize = Constants.ReceiveBufferSize;

        private Thread listenerThread;
        private Thread workerThread;
        private bool threadsRunning = false;
        private CancellationTokenSource cts;
        private CancellationToken ct;
        //  <stats
        internal long ReceivedBytes;
        internal long SentBytes;
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
        }
        /// <summary>
        /// Initializes all non-child-specific components
        /// </summary>
        internal void InitializeComponent()
        {
            cache = new Queue();
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
        }

        /// <summary>
        /// Starts the tasks for receiving and compounding
        /// </summary>
        internal void StartThreads()
        {
            if (threadsRunning) throw new InvalidOperationException("Tasks are already running.");
            threadsRunning = true;
            listenerThread = new Thread(ListenerThread);
            listenerThread.Start();
            workerThread = new Thread(WorkerThread);
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
        /// Reads data from the buffer.
        /// </summary>
        /// <param name="count">count of bytes to read</param>
        /// <exception cref="OperationCanceledException"/>
        /// <exception cref="TimeoutException"/>
        /// <returns></returns>
        internal byte[] Read(int count)
        {
            const int wait = 10;
            int cycles = (Constants.ReceiveTimeout + count / 8) / wait;
            int cycle = 0;
            while (cache.Length < count)
            {
                if (cycle >= cycles)
                    throw new TimeoutException(string.Format("Waiting for {0} bytes took over {1} ms.", count, cycles * wait));
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
            int cycles = (Constants.ReceiveTimeout + count / 8) / wait;
            int cycle = 0;
            while (cache.Length < count)
            {
                if (cycle >= cycles)
                    throw new TimeoutException(string.Format("Waiting for {0} bytes took over {1} ms.", count, cycles * wait));
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
            try
            {
                int sent = tcp.Client.Send(buf);
                if (ct.WaitHandle.WaitOne(1)) // Socket.Send() does not throw an Exception while having no connection
                    return 0;
                SentBytes += sent;
                return sent;
            }
            catch (SocketException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return 0;
            }
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
                    cts?.Dispose();
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