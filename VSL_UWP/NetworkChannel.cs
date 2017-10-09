﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace VSL
{
    /// <summary>
    /// Responsible for the network communication
    /// </summary>
    internal sealed class NetworkChannel : IDisposable
    {
        // <fields
        private VSLSocket parent;
        private StreamSocket socket;
        private Queue cache;

        private bool threadsRunning = false;
        private CancellationTokenSource cts;
        private CancellationToken ct;
        //  <stats
        internal long ReceivedBytes;
        internal long SentBytes;
        //   stats>
        //  fields>

        // <properties
        internal uint ReceiveBufferSize { get; set; } = Constants.ReceiveBufferSize;
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
        /// <param name="socket">Connected <see cref="StreamSocket"/></param>
        internal NetworkChannel(VSLSocket parent, StreamSocket socket)
        {
            this.parent = parent;
            this.socket = socket;
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
        /// <param name="socket">Connected <see cref="Socket"/></param>
        internal void Connect(StreamSocket socket)
        {
            this.socket = socket;
        }

        /// <summary>
        /// Starts the tasks for receiving and compounding
        /// </summary>
        internal void StartThreads()
        {
            if (threadsRunning) throw new InvalidOperationException("Tasks are already running.");
            threadsRunning = true;
            ListenerThread();
            WorkerThread();
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
        private async void ListenerThread()
        {
            lock (disposeLock)
                listenerReady = false;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    byte[] buf = new byte[ReceiveBufferSize];
                    IBuffer ibuf = new Windows.Storage.Streams.Buffer(ReceiveBufferSize);
                    await socket.InputStream.ReadAsync(ibuf, ReceiveBufferSize, InputStreamOptions.Partial);
                    DataReader.FromBuffer(ibuf).ReadBytes(buf);
                    if (buf.Length > 0)
                    {
                        cache.Enqeue(Crypt.Util.TakeBytes(buf, buf.Length));
                        ReceivedBytes += buf.Length;
                    }
                    else
                    {
                        await Task.Delay(parent.SleepTime);
                    }
                }
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

        /// <summary>
        /// Compounds packets from the received data
        /// </summary>
        /// <returns></returns>
        private async void WorkerThread()
        {
            try
            {
                lock (disposeLock)
                    workerReady = false;
                while (!ct.IsCancellationRequested)
                {
                    if (cache.Length > 0)
                    {
                        if (!await parent.manager.OnDataReceiveAsync())
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
        internal async Task<int> SendAsync(byte[] buf)
        {
            try
            {
                int sent = Convert.ToInt32(await socket.OutputStream.WriteAsync(buf.AsBuffer()));
                if (!await socket.OutputStream.FlushAsync())
                    return 0;
                SentBytes += sent;
                return sent;
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
        internal async Task CloseConnectionAsync()
        {
            StopThreads();
            await socket.CancelIOAsync();
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