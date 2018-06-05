﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using VSL.BinaryTools;

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

        private Timer timer;
        private volatile bool threadsRunning = false;
        private CancellationTokenSource cts;
        private CancellationToken ct;
        /// <summary>
        /// Blocks disposing until the <see cref="NetworkChannel"/> was started.
        /// </summary>
        private ManualResetEventSlim disposeHandle;
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
            disposeHandle = new ManualResetEventSlim();
            timer = new Timer(TimerWork, null, -1, -1);
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
            if (disposedValue)
                throw new ObjectDisposedException("VSL.NetworkChannel");
            if (threadsRunning)
                throw new InvalidOperationException("Threads are already running.");
            threadsRunning = true;
            StartReceive();
            timer.Change(0, -1);
            disposeHandle.Set();
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

        private void StartReceive()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[ReceiveBufferSize], 0, ReceiveBufferSize);
            args.Completed += ReceiveAsync_Completed;
            socket.ReceiveAsync(args);
        }

        private void ReceiveAsync_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError != SocketError.Success)
                {
                    threadsRunning = false;
                    if (e.SocketError != SocketError.OperationAborted)
                        parent.ExceptionHandler.CloseConnection(e.SocketError, e.LastOperation);
                }

                int len = e.BytesTransferred;
                if (len > 0)
                {
                    cache.Enqeue(e.Buffer.Take(len));
                    ReceivedBytes += len;
                }
                else if (threadsRunning)
                {
                    threadsRunning = false;
                    parent.ExceptionHandler.CloseConnection(SocketError.ConnectionReset, e.LastOperation);
                }

                if (threadsRunning)
                {
                    e.SetBuffer(0, ReceiveBufferSize);
                    if (socket != null && !socket.ReceiveAsync(e))
                        threadsRunning = false;
                }
            }
#if DEBUG
            catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
#else
            catch (Exception ex)
#endif
            {
                parent.ExceptionHandler.CloseUncaught(ex);
            }
        }

        /// <summary>
        /// Compounds, decrypts and handles packets. This is a ThreadPool/Timer implementation of the former worker thread.
        /// </summary>
        /// <param name="state"></param>
        private void TimerWork(object state)
        {
            while (threadsRunning && cache.Length > 0)
            {
                if (!parent.manager.OnDataReceive())
                    return;
            }
            if (threadsRunning)
                timer.Change(parent.SleepTime, -1);
        }

        /// <summary>
        /// Reads data from the buffer. This function is not threadsafe!
        /// </summary>
        /// <param name="buf">Buffer to store read bytes.</param>
        /// <param name="count">Count of bytes to read. This function will return false if not all bytes could be read.</param>
        /// <returns>Returns whether the function could successfully read all requested bytes.</returns>
        internal bool TryRead(out byte[] buf, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            const int wait = 10; // frequency to look for new data
            int cycles = (int)(parent.NetworkMaxLatency + 1000f * count / parent.NetworkMinBandwith) / wait;
            int cycle = 0;
            while (cache.Length < count)
            {
                if (cycle >= cycles)
                {
                    parent.ExceptionHandler.CloseConnection("Timeout",
                        $"Waiting for {count} bytes took over {cycles * wait} ms.\r\n" +
                        "\tat NetworkChannel.TryRead(out byte[], int)");
                    buf = null;
                    return false;
                }
                if (ct.WaitHandle.WaitOne(wait)) // shutting down
                {
                    buf = null;
                    return false;
                }
                cycle++;
            }
            if (!cache.Dequeue(out buf, count))
            {
                parent.ExceptionHandler.CloseConnection("DequeuingError",
                    $"Could not dequeue {count} bytes.\r\n" +
                    "\tat NetworkChannel.TryRead(out byte[], int)");
                return false;
            }
            return true;
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
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            StopThreads();
            try
            {
                socket?.Shutdown(SocketShutdown.Both);
            }
#if DEBUG
            catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
#else
            catch (Exception ex)
#endif
            {
                parent.ExceptionHandler.CloseUncaught(ex);
            }
#if WINDOWS_UWP
            socket?.Dispose();
#else
            socket?.Close();
#endif
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // -TODO: dispose managed state (managed objects).
                    StopThreads();
                    disposeHandle.Wait(1000); // wait up to 1 second for the NetworkChannel to start
                    timer?.Dispose();
                    cts?.Dispose();
                    disposeHandle?.Dispose();
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