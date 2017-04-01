using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace VSL
{
    internal class NetworkChannelClient : NetworkChannel
    {
        // <fields
        new private VSLClient parent;
        //  fields>

        // <constructor
        internal NetworkChannelClient(VSLClient parent)
        {
            this.parent = parent;
            base.parent = parent;
        }
        //  constructor>

        // <functions
        /// <summary>
        /// Connects the TCP Client asynchronously
        /// </summary>
        /// <param name="address">IP address or hostname</param>
        /// <param name="port">Port</param>
        /// <param name="serverKey">Public RSA key of the server</param>
        /// <returns></returns>
        internal async Task Connect(string address, int port, string serverKey)
        {
            // <check args
            if (string.IsNullOrEmpty(address)) throw new ArgumentNullException();
            if (port < 0 || port > 65535) throw new ArgumentOutOfRangeException();
            if (string.IsNullOrEmpty(serverKey)) throw new ArgumentNullException();
            // check args>

            // <resolve hostname
            IPAddress[] ips;
            try
            {
                ips = await Dns.GetHostAddressesAsync(address);
            }
            catch
            {
                throw new Exception("Could not resolve hostname " + address);
            }
            // resolve hostname>

            // <connect
            TcpClient tcp = new TcpClient();
            bool couldConnect = false;
            foreach (IPAddress ip in ips)
            {
                try
                {
                    await tcp.ConnectAsync(ip, port);
                    Connect(tcp);
                    couldConnect = true;
                    Console.WriteLine(ip.ToString());
                    break;
                }
                catch { }
            }
            if (!couldConnect) throw new Exception("Could not connect to the specified host");
            // connect>

            // <key exchange
            byte[] aesKey = Crypt.AES.GenerateKey();
            byte[] aesIV = Crypt.AES.GenerateIV();
            byte[] vslVersion = BitConverter.GetBytes(Constants.VSLVersionNumber);
            byte[] clientVersion = BitConverter.GetBytes(parent.TargetVersion);
            byte[] request = Crypt.Util.ConnectBytesPA(aesKey, aesIV, vslVersion, clientVersion);
            request = await Crypt.RSA.Encrypt(request, serverKey);
            SendRaw(request);
            //  key exchange>
        }
        //  functions>
    }
}