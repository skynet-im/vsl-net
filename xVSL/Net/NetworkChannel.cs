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
    internal sealed class NetworkChannel : IDisposable
    {
        // <fields
        private VSLSocket parent;
        private Socket socket;
        private Queue cache;
        private int _receiveBufferSize = Constants.ReceiveBufferSize;

        private Thread listenerThread;
        private Thread workerThread;
        private Timer timer;
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
                if (socket != null)
                    try
                    {
                        socket.ReceiveBufferSize = value;
                    }
                    catch (Exception ex)
                    {
                        if (parent.Logger.InitE)
                            parent.Logger.E(ex.ToString());
                    }
            }
        }
        private SocketAsyncEventArgsPool argsPool
        {
            get
            {
                return null;
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
        /// Initializes a new instance of the <see cref="NetworkChannel"/> class
        /// </summary>
        /// <param name="parent">Underlying <see cref="VSLSocket"/></param>
        /// <param name="socket">Connected <see cref="Socket"/></param>
        internal NetworkChannel(VSLSocket parent, Socket socket)
        {
            this.parent = parent;
            this.socket = socket;
            ReceiveBufferSize = _receiveBufferSize;
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
            timer = new Timer(TimerWork, null, -1, parent.SleepTime);
        }
        //  constructor>

        // <functions
        /// <summary>
        /// Sets a VSL socket for the specified client
        /// </summary>
        /// <param name="socket">Connected <see cref="Socket"/></param>
        internal void Connect(Socket socket)
        {
            this.socket = socket;
            ReceiveBufferSize = _receiveBufferSize;
        }

        /// <summary>
        /// Starts the tasks for receiving and compounding
        /// </summary>
        internal void StartThreads()
        {
            if (threadsRunning) throw new InvalidOperationException("Tasks are already running.");
            threadsRunning = true;
            StartReceive();
            timer.Change(0, parent.SleepTime);
            //listenerThread = new Thread(ListenerThread);
            //listenerThread.Start();
            //workerThread = new Thread(WorkerThread);
            //workerThread.Start();
        }

        /// <summary>
        /// Stops the tasks for receiving and compounding
        /// </summary>
        private void StopThreads()
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
                lock (disposeLock)
                    listenerReady = false;
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        byte[] buf = new byte[_receiveBufferSize];
                        int len = socket.Receive(buf, _receiveBufferSize, SocketFlags.None);
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
                    if (ex.SocketErrorCode != SocketError.Interrupted)
                        parent.ExceptionHandler.CloseConnection(ex);
                }
                catch (ObjectDisposedException ex)
                {
                    parent.ExceptionHandler.CloseConnection(ex);
                }
                finally
                {
                    lock (disposeLock)
                    {
                        listenerReady = true;
                        if (disposePending && ReadyToDispose)
                            Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                parent.ExceptionHandler.CloseUncaught(ex);
            }
        }

        private void StartReceive()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[ReceiveBufferSize], 0, ReceiveBufferSize);
            args.Completed += ReceiveAsync_Completed;
            socket.ReceiveAsync(args);
        }

        private void ReceiveAsync_Completed(object sender, SocketAsyncEventArgs e)
        {
            int len = e.BytesTransferred;
            if (len > 0)
            {
                cache.Enqeue(Crypt.Util.TakeBytes(e.Buffer, len));
                ReceivedBytes += len;
            }
            else
            {
                if (e.SocketError != SocketError.Success)
                    parent.ExceptionHandler.CloseConnection(e.SocketError, e.LastOperation);
            }
            if (!ct.IsCancellationRequested)
            {
                e.SetBuffer(0, ReceiveBufferSize);
                socket.ReceiveAsync(e);
            }
        }

        /// <summary>
        /// Compounds packets from the received data
        /// </summary>
        /// <returns></returns>
        private void WorkerThread()
        {
            try
            {
                lock (disposeLock)
                    workerReady = false;
                while (!ct.IsCancellationRequested)
                {
                    if (cache.Length > 0)
                    {
                        if (!parent.manager.OnDataReceive())
                            break;
                    }
                    else
                    {
                        if (ct.WaitHandle.WaitOne(parent.SleepTime))
                            break;
                    }
                }
                lock (disposeLock)
                {
                    workerReady = true;
                    if (disposePending && ReadyToDispose)
                        Dispose();
                }
            }
            catch (Exception ex)
            {
                parent.ExceptionHandler.CloseUncaught(ex);
            }
        }

        private void TimerWork(object state)
        {
            while (cache.Length > 0)
            {
                if (!parent.manager.OnDataReceive())
                    timer.Dispose();
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
                int sent = socket.Send(buf);
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
            catch (ObjectDisposedException ex)
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
            StopThreads();
            socket?.Shutdown(SocketShutdown.Both);
#if WINDOWS_UWP
            socket?.Dispose();
#else
            socket?.Close();
#endif
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private object disposeLock = new object();
        private bool listenerReady = true;
        private bool workerReady = true;
        private bool ReadyToDispose => listenerReady && workerReady;
        private bool disposePending = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                lock (disposeLock)
                {
                    if (!ReadyToDispose)
                    {
                        disposePending = true;
                        return;
                    }
                }

                if (disposing)
                {
                    // -TODO: dispose managed state (managed objects).
                    StopThreads();
                    cts?.Dispose();
                    socket?.Dispose();
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