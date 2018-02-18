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
    public partial class FrmMain : Form
    {
        private VSLClient vslClient;
        private Server server;
        private bool clientConnected;

        public FrmMain()
        {
            InitializeComponent();
            Text = string.Format(Text, Constants.ProductVersion);
            server = new Server(Program.Port, Program.Keypair);
        }

        private void BtnStartServer_Click(object sender, EventArgs e)
        {
            if (!server.Running)
            {
                btnStartServer.Enabled = false;
                server.Start();
            }
            else
            {
                btnStartServer.Enabled = true;
                server.Stop();
            }
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
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
                vslClient.PacketReceived += VslClient_Received;
                await vslClient.ConnectAsync("localhost", Program.Port, Program.PublicKey);
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
            btnReceiveFile.Enabled = true;
            btnSendFile.Enabled = true;
            clientConnected = true;
        }

        private void VSL_Close(object sender, ConnectionClosedEventArgs e)
        {
            btnConnect.Text = "Verbinden";
            btnClientSendPacket.Enabled = false;
            btnReceiveFile.Enabled = false;
            btnSendFile.Enabled = false;
            clientConnected = false;
        }

        private void BtnSendPacket_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            byte[] b = new byte[65536];
            rnd.NextBytes(b);
            if ((Button)sender == btnClientSendPacket)
                vslClient.SendPacket(1, b);
            else if ((Button)sender == btnServerSendPacket)
                Program.Clients.ParallelForEach((c) => c.Vsl.SendPacket(1, b));
        }

        private void VslClient_Received(object sender, PacketReceivedEventArgs e)
        {
            MessageBox.Show(string.Format("Client received: ID={0} Content={1}", e.ID, e.Content.Length));
        }

        private void BtnReceiveFile_Click(object sender, EventArgs e)
        {
            string path = Path.Combine("D:", "ProgramData", "VSLTest", Path.GetRandomFileName());
            MessageBox.Show(path);
            FTEventArgs args = new FTEventArgs(new Identifier(0), null, path);
            args.Progress += VslClient_FTProgress;
            args.Finished += VslClient_FTFinished;
            vslClient.FileTransfer.Download(args);
            btnReceiveFile.Enabled = false;
            btnSendFile.Enabled = false;
        }

        private void BtnSendFile_Click(object sender, EventArgs e)
        {
            string path;
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.ShowDialog();
                path = fd.FileName;
            }
            MessageBox.Show(path);
            FTEventArgs args = new FTEventArgs(new Identifier(0), new FileMeta(path), path);
            args.Progress += VslClient_FTProgress;
            args.Finished += VslClient_FTFinished;
            vslClient.FileTransfer.Upload(args);
            btnReceiveFile.Enabled = false;
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
                            tcp.Connect("::1", Program.Port);
                            Random rand = new Random();
                            byte[] buf = new byte[rand.Next(2048)];
                            rand.NextBytes(buf);
                            tcp.Client.Send(buf);
                            tcp.Close();
                            done++;
                            if (System.Diagnostics.Debugger.IsAttached)
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

        private void VslClient_FTProgress(object sender, FTProgressEventArgs e)
        {
            pbFileTransfer.Value = Convert.ToInt32(e.Percentage * 100);
        }

        private void VslClient_FTFinished(object sender, EventArgs e)
        {
            ((FTEventArgs)sender).Progress -= VslClient_FTProgress;
            ((FTEventArgs)sender).Finished -= VslClient_FTFinished;
            btnReceiveFile.Enabled = true;
            btnSendFile.Enabled = true;
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (vslClient != null)
            {
                vslClient.ConnectionClosed -= VSL_Close;
                if (clientConnected)
                    vslClient.CloseConnection("");
                else
                    vslClient.Dispose();
            }
            server.Stop();
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