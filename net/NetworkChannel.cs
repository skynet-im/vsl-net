using System;
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
    internal abstract class NetworkChannel
    {
        // <fields
        internal byte[] AesKey; // AES key for the running connection
        internal byte[] ReceiveIV; // The IV that will be used for the next decryption
        internal byte[] SendIV;
        internal VSLSocket parent;

        private TcpClient tcp;
        private Queue cache;
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
        /// Initializes all non-child-specific components
        /// </summary>
        internal void InitializeComponent()
        {
            tcp = new TcpClient() { ReceiveBufferSize = _networkBufferSize };
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
            this.tcp.ReceiveBufferSize = _networkBufferSize;
        }

        /// <summary>
        /// Starts the tasks for receiving and compounding
        /// </summary>
        private void StartTasks()
        {
            Task lt = ListenerTask(ct);
            Task wt = WorkerTask(ct);
        }

        /// <summary>
        /// Receives bytes from the socket to the cache
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ListenerTask(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    byte[] buf = new byte[_networkBufferSize - 1];
                    int len = tcp.Client.Receive(buf, _networkBufferSize, SocketFlags.None);
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
        /// Compounds packets from the received data
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task WorkerTask(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (cache.Length > 0)
                {
                    OnDataReveive(ct);
                }
                else
                {
                    await Task.Delay(10, ct);
                }
            }
        }

        internal virtual async void OnDataReveive(CancellationToken ct)
        {
            try
            {
                byte[] head = await Read(32, ct);
                head = await Crypt.AES.DecryptAsync(head, AesKey, ReceiveIV);
                PacketBuffer hreader = new PacketBuffer(head);
                byte[] iv = hreader.ReadByteArray(16);
                int length = Convert.ToInt32(hreader.ReadUInt());
                byte id = hreader.ReadByte();

                byte[] content = await Read(length, ct);
                content = await Crypt.AES.DecryptAsync(content, AesKey, iv);
                ReceiveIV = iv;
                parent.OnPacketReceived(id, content);
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine("[VSL] TimeoutException in NetworkChannel.OnDataReceive(): " + ex.ToString());
            }
        }

        /// <summary>
        /// Reads data from the buffer
        /// </summary>
        /// <param name="count">count of bytes to read</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        internal async Task<byte[]> Read(int count, CancellationToken ct)
        {
            int cycle = 1;
            while (cache.Length < count)
            {
                if (cycle >= 10) throw new TimeoutException();
                await Task.Delay(10, ct);
                cycle++;
            }
            byte[] buf = new byte[count - 1];
            bool success = cache.Dequeue(out buf, count);
            if (!success) throw new Exception("Error in the cache");
            return buf;
        }

        /// <summary>
        /// Sends a packet to the remote client
        /// </summary>
        /// <param name="id">Packet ID</param>
        /// <param name="content">Packet data</param>
        internal async void SendPacket(byte id, byte[] content)
        {
            byte[] iv = Crypt.AES.GenerateIV();
            byte[] buf = await Crypt.AES.EncryptAsync(content, AesKey, iv);
            byte[] length = BitConverter.GetBytes(Convert.ToUInt32(buf.Length));
            byte[] head = Crypt.Util.ConnectBytesPA(iv, length, new byte[1] { id });
            head = await Crypt.AES.EncryptAsync(head, AesKey, SendIV);
            byte[] data = Crypt.Util.ConnectBytesPA(head, buf);
            SendIV = iv;
            SendRaw(data);
        }

        /// <summary>
        /// Sends data to the remote client
        /// </summary>
        /// <param name="buf"></param>
        internal void SendRaw(byte[] buf)
        {
            try
            {
                tcp.Client.Send(buf);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("[VSL] SocketException in NetworkChannel.Send(): " + ex.ToString());
            }
        }
        //  functions>
    }
}