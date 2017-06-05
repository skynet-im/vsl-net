using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSL;
using VSL.FileTransfer;

namespace VSLTest
{
    public partial class frmMain : Form
    {
        private VSLServer vslServer;
        private VSLClient vslClient;
        private const string keypair = "<RSAKeyValue><Modulus>qBQQScN/+A2Tfi971dmOyPwSXpoq3XVwQBJNzbCCk1ohGVoOzdNK87Csw3thZyrynfaDzujW555S4HkWXxLR5dzo8rj/6KAk0yugYtFMt10XC1iZHRQACQIB3j+lS5wK9ZHfbsE4+/CUAoUdhYa9cad/xEbYrgkkyY0TuZZ1w2piiE1SdOXB+U6NF1aJbkUtKrHU2zcp5YzhYlRePvx7e+GQ5GMctSuT/xFzPpBZ5DZx1I/7lQicq7V21M/ktilRQIeqIslX98j4jLuFriinySwW+oi0s+8hantRwZ9jgAIIEao9+tbDSj8ePHb0Li6hhuoMmLeImLaoadDG39VnFQ==</Modulus><Exponent>AQAB</Exponent><P>z5tqnHWJ4X0lZVBXtVxJNI/h5NLBrwDoS0bvFL0Mx1SkYjF4Rjy8EGrLA51W+C8TzEY7ddBiC+eQPXw1PlbzHg+h0hal2gp5iUj+QEvCw1vDVXGoGTeP6UBL8ixYTbLQaVG70rWPm7j2nR7sQSQgJHX4ppvKQ4Mo9DI1RnJ1/2U=</P><Q>z0HXU22CFiUhoxuWuePjtkJ2gtopsZu5x6F/I+EqBqnq8KVVp+qRKOHm34xbh3gTQjDcBtJXu+FGgKRvQWj5ArhMt2QtNKIhmRBIuRQoHWSwg0deMPzD9IUHDU8D4xwkoZWuAGFjWW5KrkW6TX6SMHM8GUMnGzGP50MbIrEHBfE=</Q><DP>zvoJbfcZAb+82qcg6mUZbtfLxFACXTEwZmxPy4M3DDtsr6DWYmAGtu9hezcQD9sPh+a1PR4FwgyZF1OP2ZjiRSQcltGRhDJRPPeS1BM0F4SS18q6Znmodklt7gEcAEq30Wh1MvtkM0JSTA8aR0925CLhRWmoW2qWF+8+gf93eKk=</DP><DQ>U+5p8NMsFyO6V39YrrbnBGwt6hfHQrG5rmpsPm90wXYWOpX59iI73r587JK+jkHGKsv2jpyoAuHb10S/+VE1ZjCUgMAEvofZ60545NqQ1DZudPt13Yi/Ikqs7GrPPC2td/JRoL3PqevMOn7qT2+ubAh+kgxrzctoZ1L5rjbajUE=</DQ><InverseQ>o/VbhG+A+MtSe1qNCsgv41bCSVVJyzJH+lC/j3hYksjwFJEimDu6D+MheFU/PcBER1IoomUnyUwqYfK7YLmb3JHt9nCmnUUx+OrOT81TRhs63kGm2UKMwY7vNOIvhjfsbmoeTr0Of0Mc/Pf62lp1PzJaJtCao67zC5VTLt+e16I=</InverseQ><D>BkuXSMmYzvr9/n17gajwCZqZYVY1/n/1NM0kTizLIzo+hmzPV6NPMB2HejXlkf/mwO0roCt4tLzcshnCJJleAVV65/AI071ymHJoNwAYXVjQMcvyeWD9pFi6wBVTSCe/m4i7nRiBg7w0MWKR41jgQRpeAhIjCcrmLnwvrcvGVhiXLys4vw/XEPEc5Yk7ZWUVHRDr/2f1+AEL1T7kkDPY002qIDrP2NJbRGMpNulDt1xB1qcnK0VLgQ87zOTzZEUQviYCgvZjf3xnkYG1j87acaFQlNMN6pqJGAdD158rATy99OzScORgKbYNXtx1GGc1Yzj+alaszH3xBOpghTSscQ==</D></RSAKeyValue>";
        private const string publickey = "<RSAKeyValue><Modulus>qBQQScN/+A2Tfi971dmOyPwSXpoq3XVwQBJNzbCCk1ohGVoOzdNK87Csw3thZyrynfaDzujW555S4HkWXxLR5dzo8rj/6KAk0yugYtFMt10XC1iZHRQACQIB3j+lS5wK9ZHfbsE4+/CUAoUdhYa9cad/xEbYrgkkyY0TuZZ1w2piiE1SdOXB+U6NF1aJbkUtKrHU2zcp5YzhYlRePvx7e+GQ5GMctSuT/xFzPpBZ5DZx1I/7lQicq7V21M/ktilRQIeqIslX98j4jLuFriinySwW+oi0s+8hantRwZ9jgAIIEao9+tbDSj8ePHb0Li6hhuoMmLeImLaoadDG39VnFQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private bool clientConnected = false;
        private bool serverRunning = false;
        private bool running = true;
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            if (!serverRunning)
            {
                btnStartServer.Enabled = false;
                Task t = ListenerTask();
            }
            else
            {
                vslServer.CloseConnection("Server beendet");
                running = false;
            }
        }

        private async Task ListenerTask()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 32771);
            listener.Start();
            btnStartServer.Text = "Server beenden";
            btnStartServer.Enabled = true;
            while (running)
            {
                if (listener.Pending())
                {
                    vslServer = new VSLServer(await listener.AcceptTcpClientAsync(), 0, 0, keypair);
                    //vslServer.Logger.DebugMessages = true;
                    //vslServer.Logger.ExceptionMessages = true;
                    //vslServer.Logger.InfoMessages = true;
                    vslServer.PacketReceived += vslServer_Received;
                    vslServer.ConnectionClosed += VSL_Close;
                    vslServer.FileTransfer.FileTransferRequested += vslServer_FTRequest;
                    serverRunning = true;
                }
                else
                    await Task.Delay(10);
            }
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (!clientConnected)
            {
                vslClient = new VSLClient(0, 0);
                //vslClient.Logger.DebugMessages = true;
                //vslClient.Logger.ExceptionMessages = true;
                //vslClient.Logger.InfoMessages = true;
                vslClient.ConnectionEstablished += VSL_Open;
                vslClient.ConnectionClosed += VSL_Close;
                vslClient.FileTransfer.FileTransferProgress += vslClient_FTProgress;
                vslClient.FileTransfer.FileTransferFinished += vslClient_FTFinished;
                await vslClient.ConnectAsync("localhost", 32771, publickey);
                btnConnect.Enabled = false;
            }
            else
                vslClient.CloseConnection("The client disconnected");
        }

        private void VSL_Open(object sender, EventArgs e)
        {
            btnConnect.Enabled = true;
            btnConnect.Text = "Trennen";
            btnSendFile.Enabled = true;
            clientConnected = true;
        }

        private void VSL_Close(object sender, ConnectionClosedEventArgs e)
        {
            if (sender == vslClient)
            {
                btnConnect.Text = "Verbinden";
                btnSendFile.Enabled = false;
                clientConnected = false;
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

        private void vslServer_FTRequest(object sender, EventArgs e)
        {
            if (vslServer.FileTransfer.Mode != StreamMode.UploadFile)
            {
                using (OpenFileDialog fd = new OpenFileDialog())
                {
                    fd.ShowDialog();
                    vslServer.FileTransfer.Path = fd.FileName;
                    vslServer.FileTransfer.Accept();
                }
            }
            else
            {
                vslServer.FileTransfer.Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.GetRandomFileName());
                vslServer.FileTransfer.Accept();
            }
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            //vslClient.FileTransfer.Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.GetRandomFileName());
            //vslClient.FileTransfer.RequestFile(new VSL.FileTransfer.Identifier(0), VSL.FileTransfer.StreamMode.GetFile);
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.ShowDialog();
                vslClient.FileTransfer.Path = fd.FileName;
            }
            vslClient.FileTransfer.RequestFile(new Identifier(0), StreamMode.UploadFile);
            btnSendFile.Enabled = false;
        }

        private void vslClient_FTProgress(object sender, FileTransferProgressEventArgs e)
        {
            pbFileTransfer.Value = Convert.ToInt32(e.Percentage * 100);
        }

        private void vslClient_FTFinished(object sender, EventArgs e)
        {
            btnSendFile.Enabled = true;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            vslClient.ConnectionClosed -= VSL_Close;
            vslServer?.CloseConnection("");
            vslClient?.CloseConnection("");
        }
    }
}