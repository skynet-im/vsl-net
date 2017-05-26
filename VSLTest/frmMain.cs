using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSL;
using VSL.Crypt;

namespace VSLTest
{
    public partial class frmMain : Form
    {
        private VSLServer vslServer;
        private VSLClient vslClient;
        private const string keypair = "<RSAKeyValue><Modulus>qBQQScN/+A2Tfi971dmOyPwSXpoq3XVwQBJNzbCCk1ohGVoOzdNK87Csw3thZyrynfaDzujW555S4HkWXxLR5dzo8rj/6KAk0yugYtFMt10XC1iZHRQACQIB3j+lS5wK9ZHfbsE4+/CUAoUdhYa9cad/xEbYrgkkyY0TuZZ1w2piiE1SdOXB+U6NF1aJbkUtKrHU2zcp5YzhYlRePvx7e+GQ5GMctSuT/xFzPpBZ5DZx1I/7lQicq7V21M/ktilRQIeqIslX98j4jLuFriinySwW+oi0s+8hantRwZ9jgAIIEao9+tbDSj8ePHb0Li6hhuoMmLeImLaoadDG39VnFQ==</Modulus><Exponent>AQAB</Exponent><P>z5tqnHWJ4X0lZVBXtVxJNI/h5NLBrwDoS0bvFL0Mx1SkYjF4Rjy8EGrLA51W+C8TzEY7ddBiC+eQPXw1PlbzHg+h0hal2gp5iUj+QEvCw1vDVXGoGTeP6UBL8ixYTbLQaVG70rWPm7j2nR7sQSQgJHX4ppvKQ4Mo9DI1RnJ1/2U=</P><Q>z0HXU22CFiUhoxuWuePjtkJ2gtopsZu5x6F/I+EqBqnq8KVVp+qRKOHm34xbh3gTQjDcBtJXu+FGgKRvQWj5ArhMt2QtNKIhmRBIuRQoHWSwg0deMPzD9IUHDU8D4xwkoZWuAGFjWW5KrkW6TX6SMHM8GUMnGzGP50MbIrEHBfE=</Q><DP>zvoJbfcZAb+82qcg6mUZbtfLxFACXTEwZmxPy4M3DDtsr6DWYmAGtu9hezcQD9sPh+a1PR4FwgyZF1OP2ZjiRSQcltGRhDJRPPeS1BM0F4SS18q6Znmodklt7gEcAEq30Wh1MvtkM0JSTA8aR0925CLhRWmoW2qWF+8+gf93eKk=</DP><DQ>U+5p8NMsFyO6V39YrrbnBGwt6hfHQrG5rmpsPm90wXYWOpX59iI73r587JK+jkHGKsv2jpyoAuHb10S/+VE1ZjCUgMAEvofZ60545NqQ1DZudPt13Yi/Ikqs7GrPPC2td/JRoL3PqevMOn7qT2+ubAh+kgxrzctoZ1L5rjbajUE=</DQ><InverseQ>o/VbhG+A+MtSe1qNCsgv41bCSVVJyzJH+lC/j3hYksjwFJEimDu6D+MheFU/PcBER1IoomUnyUwqYfK7YLmb3JHt9nCmnUUx+OrOT81TRhs63kGm2UKMwY7vNOIvhjfsbmoeTr0Of0Mc/Pf62lp1PzJaJtCao67zC5VTLt+e16I=</InverseQ><D>BkuXSMmYzvr9/n17gajwCZqZYVY1/n/1NM0kTizLIzo+hmzPV6NPMB2HejXlkf/mwO0roCt4tLzcshnCJJleAVV65/AI071ymHJoNwAYXVjQMcvyeWD9pFi6wBVTSCe/m4i7nRiBg7w0MWKR41jgQRpeAhIjCcrmLnwvrcvGVhiXLys4vw/XEPEc5Yk7ZWUVHRDr/2f1+AEL1T7kkDPY002qIDrP2NJbRGMpNulDt1xB1qcnK0VLgQ87zOTzZEUQviYCgvZjf3xnkYG1j87acaFQlNMN6pqJGAdD158rATy99OzScORgKbYNXtx1GGc1Yzj+alaszH3xBOpghTSscQ==</D></RSAKeyValue>";
        private const string publickey = "<RSAKeyValue><Modulus>qBQQScN/+A2Tfi971dmOyPwSXpoq3XVwQBJNzbCCk1ohGVoOzdNK87Csw3thZyrynfaDzujW555S4HkWXxLR5dzo8rj/6KAk0yugYtFMt10XC1iZHRQACQIB3j+lS5wK9ZHfbsE4+/CUAoUdhYa9cad/xEbYrgkkyY0TuZZ1w2piiE1SdOXB+U6NF1aJbkUtKrHU2zcp5YzhYlRePvx7e+GQ5GMctSuT/xFzPpBZ5DZx1I/7lQicq7V21M/ktilRQIeqIslX98j4jLuFriinySwW+oi0s+8hantRwZ9jgAIIEao9+tbDSj8ePHb0Li6hhuoMmLeImLaoadDG39VnFQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private byte[] buffer = new byte[0];
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            Task t = ListenerTask();
            btnStartServer.Enabled = false;
        }

        private async Task ListenerTask()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 32771);
            listener.Start();
            while (true)
            {
                if (listener.Pending())
                {
                    vslServer = new VSLServer(await listener.AcceptTcpClientAsync(), 0, 0, keypair);
                    vslServer.PacketReceived += vslServer_Received;
                    vslServer.ConnectionClosed += VSL_Close;
                }
                else
                    await Task.Delay(10);
            }
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            vslClient = new VSLClient(0, 0);
            vslClient.ConnectionEstablished += VSL_Open;
            vslClient.ConnectionClosed += VSL_Close;
            await vslClient.ConnectAsync("localhost", 32771, publickey);
        }

        private void VSL_Open(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
        }

        private void VSL_Close(object sender, ConnectionClosedEventArgs e)
        {
            if (sender == vslClient)
            {
                btnConnect.Enabled = true;
                MessageBox.Show("[Client] Connection closed");
            }
            else if (sender == vslServer)
                MessageBox.Show("[Server] Connection closed");
        }

        private void btnSendPacket_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            byte[] b = new byte[128];
            rnd.NextBytes(b);
            vslClient.SendPacket(1, b);
        }

        private void vslServer_Received(object sender, PacketReceivedEventArgs e)
        {
            MessageBox.Show(string.Format("Server received: ID={0} Content={1}", e.ID, VSL.Crypt.Util.ToHexString(e.Content)));
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            for (int i = 1; i <= 128; i++)
            {
                byte[] buf = new byte[i];
                rnd.NextBytes(buf);
                await SendPacketAsync(2, buf);
                if (!Util.ByteArraysEqual(await ReceivePacket_AES_256(), buf)) MessageBox.Show("Failed Packet with length " + i);
            }
        }

        private byte[] Read(int count)
        {
            byte[] buf = buffer.Take(count).ToArray();
            buffer = buffer.Skip(count).ToArray();
            return buf;
        }

        private void Send(byte[] buf)
        {
            buffer = buffer.Concat(buf).ToArray();
        }

        private byte[] AesKey { get; } = Util.GetBytes("91b67fffa84f93ef9f9881e543827c5e79da45d07d72363ed503b4c7366200af");
        private byte[] SendIV { get; } = Util.GetBytes("00000000000000000000000000000000");
        private byte[] ReceiveIV { get; } = Util.GetBytes("00000000000000000000000000000000");

        private async Task<byte[]> ReceivePacket_AES_256()
        {
            byte[] ciphertext = Read(16); //TimeoutException
            Console.WriteLine("receiving AES packet:" + Util.ToHexString(ciphertext));
            byte[] plaintext = await AES.DecryptAsync(ciphertext, AesKey, ReceiveIV); //CryptographicException
            Console.WriteLine("decrypted packet: " + Util.ToHexString(plaintext));
            byte id = plaintext.Take(1).ToArray()[0];
            plaintext = plaintext.Skip(1).ToArray();
            uint length = BitConverter.ToUInt32(plaintext.Take(4).ToArray(), 0);
            plaintext = plaintext.Skip(4).ToArray();
            Console.WriteLine("AES Packet length=" + length);
            if (length > plaintext.Length - 2) // 2 random bytes in the header for more security
            {
                int pendingLength = Convert.ToInt32(length - plaintext.Length + 2);
                Console.WriteLine("AES Packet pending length=" + pendingLength);
                int pendingBlocks = Convert.ToInt32(Math.Ceiling((pendingLength + 1) / 16d)); // round up, first blocks only 15 bytes (padding)
                Console.WriteLine("AES Packet pending blocks=" + pendingBlocks);
                ciphertext = Read(pendingBlocks * 16);
                Console.WriteLine("AES Packet next ciphertext =" + Util.ToHexString(ciphertext));
                plaintext = plaintext.Concat(await AES.DecryptAsync(ciphertext, AesKey, ReceiveIV)).ToArray();
            }
            int startIndex = Convert.ToInt32(plaintext.Length - length);
            byte[] content = plaintext.Skip(startIndex).ToArray(); // remove random bytes
            return content;
        }
        private Task SendPacketAsync(byte id, byte[] content)
        {
            byte[] head = new byte[1] { id };
            head = head.Concat(BitConverter.GetBytes(Convert.ToUInt32(content.Length))).ToArray();
            return SendPacketAsync_AES_256(head, content);
        }
        private async Task SendPacketAsync_AES_256(byte[] head, byte[] content)
        {
            int blocks;
            int saltLength;
            if (head.Length + 2 + content.Length < 16)
            {
                blocks = 1;
                saltLength = 15 - head.Length - content.Length;
            }
            else
            {
                blocks = Convert.ToInt32(Math.Ceiling((head.Length + 4 + content.Length) / 16d)); //at least 2 random bytes in the header block
                saltLength = blocks * 16 - head.Length - content.Length - 2; //first blocks only 15 bytes (padding)
            }
            Console.WriteLine("salt length: " + Convert.ToString(saltLength));
            byte[] salt = new byte[saltLength];
            Random random = new Random();
            random.NextBytes(salt);
            byte[] plaintext = Util.ConnectBytesPA(head, salt, content);
            Console.WriteLine("sending AES packet: " + Util.ToHexString(plaintext));
            byte[] headBlock = await AES.EncryptAsync(plaintext.Take(15).ToArray(), AesKey, SendIV);
            byte[] tailBlock = new byte[0];
            if (plaintext.Length > 15)
            {
                plaintext = plaintext.Skip(15).ToArray();
                tailBlock = await AES.EncryptAsync(plaintext, AesKey, SendIV);
            }
            Console.WriteLine("encrypted packet: " + Util.ToHexString(Util.ConnectBytesPA(headBlock, tailBlock)));
            Send(Util.ConnectBytesPA(headBlock, tailBlock));
        }
    }
}