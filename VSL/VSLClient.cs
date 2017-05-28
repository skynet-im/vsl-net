using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using VSL.FileTransfer;

namespace VSL
{
    /// <summary>
    /// The client implementation of a VSL socket
    /// </summary>
    public class VSLClient : VSLSocket
    {
        // <fields
        new internal NetworkManagerClient manager;
        new internal PacketHandlerClient handler;
        /// <summary>
        /// Access file transfer functions.
        /// </summary>
        new public FileTransferClient FileTransfer;
        internal ushort LatestProduct;
        internal ushort OldestProduct;
        //  fields>
        // <constructor
        /// <summary>
        /// Creates a VSL Client that has to be connected
        /// </summary>
        /// <param name="latestProduct">The application version</param>
        /// <param name="oldestProduct">The oldest supported version</param>
        public VSLClient(ushort latestProduct, ushort oldestProduct)
        {
            InitializeComponent();

            LatestProduct = latestProduct;
            OldestProduct = oldestProduct;
        }
        // constructor>
        // <functions
        /// <summary>
        /// Connects the TCP Client asynchronously
        /// </summary>
        /// <param name="address">IP address or hostname</param>
        /// <param name="port">Port</param>
        /// <param name="serverKey">Public RSA key of the server</param>
        /// <returns></returns>
        public async Task ConnectAsync(string address, int port, string serverKey)
        {
            // <check args
            if (string.IsNullOrEmpty(address)) throw new ArgumentNullException();
            if (port < 0 || port > 65535) throw new ArgumentOutOfRangeException();
            if (string.IsNullOrEmpty(serverKey)) throw new ArgumentNullException();
            //  check args>

            TcpClient tcp = new TcpClient();
            await tcp.ConnectAsync(address, port);

            // <initialize component
            channel = new NetworkChannel(this, tcp);
            manager = new NetworkManagerClient(this, serverKey);
            base.manager = manager;
            handler = new PacketHandlerClient(this);
            base.handler = handler;
            FileTransfer = new FileTransferClient(this);
            base.FileTransfer = FileTransfer;
            //  initialize component>


            // <resolve hostname
            /*IPAddress[] ips;
            try
            {
                ips = await Dns.GetHostAddressesAsync(address);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not resolve hostname " + address + ": " + ex);
            }*/
            // resolve hostname>

            // <connect
            /*bool couldConnect = false;
            foreach (IPAddress ip in ips)
            {
                try
                {
                    await tcp.ConnectAsync(ip, port);
                    channel.Connect(tcp);
                    couldConnect = true;
                    Console.WriteLine(ip.ToString());
                    break;
                }
                catch { }
            }
            if (!couldConnect) throw new Exception("Could not connect to the specified host");*/
            // connect>

            // <key exchange
            Task s = manager.SendPacketAsync(CryptographicAlgorithm.None, new Packet.P00Handshake(Packet.RequestType.DirectPublicKey));
            Task<byte[]> key = Task.Run(() => Crypt.AES.GenerateKey());
            Task<byte[]> civ = Task.Run(() => Crypt.AES.GenerateIV());
            Task<byte[]> siv = Task.Run(() => Crypt.AES.GenerateIV());
            manager.AesKey = await key;
            manager.SendIV = await civ;
            manager.ReceiveIV = await siv;
            await s;
            await manager.SendPacketAsync(CryptographicAlgorithm.RSA_2048, new Packet.P01KeyExchange(manager.AesKey, manager.SendIV,
                manager.ReceiveIV, Constants.VersionNumber, Constants.CompatibilityVersion, LatestProduct, OldestProduct));
            //  key exchange>
        }
        //  functions>
    }
}