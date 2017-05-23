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
    internal class NetworkChannel
    {
        // <fields
        internal VSLSocket parent;
        private TcpClient tcp;
        private Queue cache;
        private ConcurrentQueue<byte[]> queue;
        private CancellationTokenSource cts;
        private CancellationToken ct;
        private int _networkBufferSize = 65536;
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
            //Task lt = ListenerTask();
            Thread lt = new Thread(() => ListenerThread()); lt.Start();
            Task wt = WorkerTask();
            Task st = SenderTask();
        }

        /// <summary>
        /// Stops the tasks for receiving and compounding
        /// </summary>
        private void StopTasks()
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }

        /// <summary>
        /// Receives bytes from the socket to the cache
        /// </summary>
        /// <returns></returns>
        private async Task ListenerTask()
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    byte[] buf = new byte[_networkBufferSize];
                    int len = tcp.Client.Receive(buf, _networkBufferSize, SocketFlags.None); //Does not work asynchronously
                    if (len == 0)
                    {
                        await Task.Delay(10);
                        continue;
                    }
                    cache.Enqeue(buf.Take(len).ToArray());
                }
                catch { }
            }
        }
        /// <summary>
        /// Receives bytes from the socket to the cache
        /// </summary>
        /// <returns></returns>
        private void ListenerThread()
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    byte[] buf = new byte[_networkBufferSize];
                    int len = tcp.Client.Receive(buf, _networkBufferSize, SocketFlags.None); //Does not work asynchronously
                    if (len == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    cache.Enqeue(buf.Take(len).ToArray());
                }
                catch (SocketException ex)
                {
                    parent.ExceptionHandler.HandleSocketException(ex);
                }
            }
        }

        /// <summary>
        /// Compounds packets from the received data
        /// </summary>
        /// <returns></returns>
        private async Task WorkerTask()
        {
            while (!ct.IsCancellationRequested)
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
                        SendRaw(buf);
                    }
                    else
                    {
                        Console.WriteLine("[VSL] Error at deqeuing the send queue in NetworkChannel.SenderTask");
                    }
                }
                else
                {
                    await Task.Delay(10, ct);
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
                if (cycle >= 10) throw new TimeoutException();
                await Task.Delay(10, ct);
                cycle++;
            }
            byte[] buf = new byte[count];
            bool success = cache.Dequeue(out buf, count);
            if (!success) throw new Exception("Error in the cache");
            return buf;
        }
        /// <summary>
        /// Reads data from the buffer
        /// </summary>
        /// <param name="count">count of bytes to read</param>
        /// <returns></returns>
        internal Task<byte[]> ReadAsync(uint count)
        {
            return ReadAsync(Convert.ToInt32(count));
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
            }
            catch (SocketException ex)
            {
                Console.WriteLine("[VSL] SocketException in NetworkChannel.SendRaw(): " + ex.ToString());
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
        /// <param name="reason"></param>
        internal void CloseConnection(string reason)
        {
            StopTasks();
            tcp.Close();
        }
        //  functions>
    }
}