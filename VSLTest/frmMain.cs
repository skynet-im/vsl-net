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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSL;
using VSL.FileTransfer;

namespace VSLTest
{
    public partial class frmMain : Form
    {
        private VSLClient vslClient;
        // TESTSEVER: 
        private const string publickey = "<RSAKeyValue><Modulus>qBQQScN/+A2Tfi971dmOyPwSXpoq3XVwQBJNzbCCk1ohGVoOzdNK87Csw3thZyrynfaDzujW555S4HkWXxLR5dzo8rj/6KAk0yugYtFMt10XC1iZHRQACQIB3j+lS5wK9ZHfbsE4+/CUAoUdhYa9cad/xEbYrgkkyY0TuZZ1w2piiE1SdOXB+U6NF1aJbkUtKrHU2zcp5YzhYlRePvx7e+GQ5GMctSuT/xFzPpBZ5DZx1I/7lQicq7V21M/ktilRQIeqIslX98j4jLuFriinySwW+oi0s+8hantRwZ9jgAIIEao9+tbDSj8ePHb0Li6hhuoMmLeImLaoadDG39VnFQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private const int port = 32761;
        private bool clientConnected = false;
        private bool serverRunning = false;
        private volatile bool running = true;
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            if (!serverRunning)
            {
                btnStartServer.Enabled = false;
                ListenerTask();
            }
            else
            {
                CloseServer();
            }
        }

        private void ListenerTask()
        {
            //Socket listener = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            //TcpListener listener = new TcpListener(IPAddress.Any, 32771);
            TcpListener listener = new TcpListener(IPAddress.IPv6Loopback, port);
            listener.Server.DualMode = true;
            listener.Start();
            btnStartServer.Text = "Beenden";
            btnStartServer.Enabled = true;
            serverRunning = true;
            ThreadPool.QueueUserWorkItem((o) => 
            {
                while (running)
                {
                    Socket native = listener.AcceptSocket();
                    Client c = new Client(native);
                    Interlocked.Increment(ref Program.Connects);
                }
            });
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (!clientConnected)
            {
                vslClient = new VSLClient(0, 0);
                vslClient.Logger.PrintDebugMessages = true;
                vslClient.Logger.PrintExceptionMessages = true;
                vslClient.Logger.PrintInfoMessages = true;
                vslClient.Logger.PrintUncaughtExceptions = true;
                vslClient.ConnectionEstablished += VSL_Open;
                vslClient.ConnectionClosed += VSL_Close;
                vslClient.PacketReceived += vslClient_Received;
                vslClient.FileTransfer.FileTransferProgress += vslClient_FTProgress;
                vslClient.FileTransfer.FileTransferFinished += vslClient_FTFinished;
                await vslClient.ConnectAsync("localhost", port, publickey);
                btnConnect.Enabled = false;
            }
            else
                vslClient.CloseConnection("The client disconnected");
        }

        private void VSL_Open(object sender, EventArgs e)
        {
            btnConnect.Enabled = true;
            btnConnect.Text = "Trennen";
            btnClientSendPacket.Enabled = true;
            btnSendFile.Enabled = true;
            clientConnected = true;
        }

        private void VSL_Close(object sender, ConnectionClosedEventArgs e)
        {
            btnConnect.Text = "Verbinden";
            btnClientSendPacket.Enabled = false;
            btnSendFile.Enabled = false;
            clientConnected = false;
            //MessageBox.Show(string.Format("[Client] Connection closed\r\nReason: {0}\r\nReceived: {1}\r\nSent: {2}", e.Reason, e.ReceivedBytes, e.SentBytes));
        }

        private void btnSendPacket_Click(object sender, EventArgs e)
        {
            // Skynet:
            //PacketBuffer buf = new PacketBuffer();
            //buf.WriteString("Twometer");
            //buf.WriteByteArray(new byte[32], false);
            //buf.WriteDate(DateTime.Now);
            //buf.WriteDate(DateTime.Now.AddDays(-1));
            //byte[] contents = buf.ToArray();
            //vslClient.SendPacket(1, contents);

            // VSL:
            Random rnd = new Random();
            byte[] b = new byte[65536];
            rnd.NextBytes(b);
            if ((Button)sender == btnClientSendPacket)
                vslClient.SendPacket(1, b);
            else if ((Button)sender == btnServerSendPacket)
                Program.Clients.ParallelForEach((c) => c.Vsl.SendPacket(1, b));
            //foreach (Client c in Program.Clients)
            //{
            //    c.Vsl.SendPacket(1, b);
            //}
        }

        private void vslClient_Received(object sender, PacketReceivedEventArgs e)
        {
            MessageBox.Show(string.Format("Client received: ID={0} Content={1}", e.ID, e.Content.Length));
        }

        private void vslServer_Received(object sender, PacketReceivedEventArgs e)
        {
            if (e.Content.Length > 1024)
                MessageBox.Show(string.Format("Server received: ID={0} Content={1}", e.ID, e.Content.Length));
            else
                MessageBox.Show(string.Format("Server received: ID={0} Content={1}", e.ID, VSL.Crypt.Util.ToHexString(e.Content)));
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            //using (OpenFileDialog fd = new OpenFileDialog())
            //{
            //    fd.ShowDialog();
            //    if (string.IsNullOrEmpty(fd.FileName))
            //    {
            //        fd.Dispose();
            //        return;
            //    }
            //    vslClient.FileTransfer.Path = fd.FileName;
            //}
            vslClient.FileTransfer.Path = Path.Combine("D:", "ProgramData", "VSLTest", Path.GetRandomFileName());
            MessageBox.Show(vslClient.FileTransfer.Path);
            vslClient.FileTransfer.RequestFile(new Identifier(0), StreamMode.GetFile);
            btnSendFile.Enabled = false;
        }

        private volatile bool runningPT = false;
        private void BtnPenetrationTest_Click(object sender, EventArgs e)
        {
            if (runningPT)
            {
                runningPT = false;
                btnPenetrationTest.Text = "Stresstest";
            }
            else
            {
                runningPT = true;
                btnPenetrationTest.Text = "Stoppen";
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                    int done = 0;
                    while (runningPT && done < 1000)
                    {
                        try
                        {
                            TcpClient tcp = new TcpClient(AddressFamily.InterNetworkV6);
                            tcp.Connect("::1", port);
                            Random rand = new Random();
                            byte[] buf = new byte[rand.Next(2048)];
                            rand.NextBytes(buf);
                            tcp.Client.Send(buf);
                            tcp.Close();
                            done++;
                            Thread.Sleep(10);
                        }
                        catch { }
                    }
                    stopwatch.Stop();
                    MessageBox.Show(string.Format("1000 attacks in {0}ms", stopwatch.ElapsedMilliseconds));
                });
            }
        }

        private void BtnCleanup_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Program.Clients.Cleanup();
            stopwatch.Stop();
            MessageBox.Show(string.Format("Cleanup successful after {0} ms.", stopwatch.ElapsedMilliseconds));
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
            if (vslClient != null)
            {
                vslClient.ConnectionClosed -= VSL_Close;
                try
                {
                    vslClient.CloseConnection("");
                }
                catch (ObjectDisposedException) { }
            }
            CloseServer();
        }

        private void CloseServer()
        {
            Program.Clients.ParallelForEach((c) =>
            {
                try
                {
                    c.Vsl.CloseConnection("");
                }
                catch { }
            });
            running = false;
        }

        private int maxCount = 0;
        private void LbServerUpdateTimer_Tick(object sender, EventArgs e)
        {
            int current = Program.Clients.Count;
            if (current > maxCount)
                maxCount = current;
            LbServer.Text = string.Format("{0} / {1} Clients\r\nConnects: {2} <> {3}", current, maxCount, Program.Connects, Program.Disconnects);
        }
    }
}