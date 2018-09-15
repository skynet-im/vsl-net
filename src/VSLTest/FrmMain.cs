using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSL;
using VSL.BinaryTools;
using VSL.Crypt;
using VSL.FileTransfer;

namespace VSLTest
{
    public partial class FrmMain : Form
    {
        private VSLClient vslClient;
        private Server server;
        private PenetrationTest pentest;
        private bool clientConnected;

        public FrmMain()
        {
            InitializeComponent();
            ToolTipMain.SetToolTip(LbFileKey, "Zum Generieren eines Schlüssels klicken.");
            Text = string.Format(Text, Assembly.GetAssembly(typeof(VSLSocket)).GetName().Version);
            server = new Server(Program.Port, Program.Keypair);
            pentest = new PenetrationTest();
        }

        private void BtnStartServer_Click(object sender, EventArgs e)
        {
            if (!server.Running)
            {
                btnStartServer.Enabled = false;
                CbLocalhost.Enabled = false;
                server.Start(CbLocalhost.Checked);
                btnStartServer.Text = "Server stoppen";
                btnStartServer.Enabled = true;
            }
            else
            {
                btnStartServer.Enabled = false;
                server.Stop();
                btnStartServer.Text = "Server starten";
                btnStartServer.Enabled = true;
                CbLocalhost.Enabled = true;
            }
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            if (!clientConnected)
            {
                btnConnect.Enabled = false;
                vslClient = new VSLClient(0, 0);
                vslClient.Logger.PrintDebugMessages = true;
                vslClient.Logger.PrintExceptionMessages = true;
                vslClient.Logger.PrintInfoMessages = true;
                vslClient.Logger.PrintUncaughtExceptions = true;
                vslClient.Settings = new SocketSettings() { CatchApplicationExceptions = false };
                vslClient.ConnectionEstablished += VSL_Open;
                vslClient.ConnectionClosed += VSL_Close;
                vslClient.PacketReceived += VslClient_Received;
                var progress = new Progress<VSLClient.ConnectionState>((state) => Console.WriteLine(state));
                await vslClient.ConnectAsync("localhost", Program.Port, Program.PublicKey, progress);
            }
            else
                vslClient.CloseConnection("The user requested to disconnect", null);
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
                vslClient.SendPacketAsync(1, b);
            else if ((Button)sender == btnServerSendPacket)
                Program.Clients.ParallelForEach((c) => c.SendPacket(1, b));
        }

        private void VslClient_Received(object sender, PacketReceivedEventArgs e)
        {
            MessageBox.Show(string.Format("Client received: ID={0} Content={1}", e.Id, e.Content.Length));
        }

        private void BtnReceiveFile_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Program.TempPath, Path.GetRandomFileName());
            MessageBox.Show(path);
            FTEventArgs args = new FTEventArgs(new Identifier(0), null, path);
            args.Progress += VslClient_FTProgress;
            args.Finished += VslClient_FTFinished;
            args.FileMetaReceived += VslClient_FTFileMetaReceived;
            vslClient.FileTransfer.StartDownloadAsync(args);
            btnReceiveFile.Enabled = false;
            btnSendFile.Enabled = false;
        }

        private async void BtnSendFile_Click(object sender, EventArgs e)
        {
            string path;
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.InitialDirectory = Program.TempPath;
                if (fd.ShowDialog() == DialogResult.Cancel) return;
                path = fd.FileName;
            }
            MessageBox.Show(path);

            ContentAlgorithm algorithm = ContentAlgorithm.None;
            byte[] aesKey = null;
            byte[] hmacKey = null;
            if (!string.IsNullOrWhiteSpace(TbFileKey.Text))
            {
                algorithm = ContentAlgorithm.Aes256CbcHmacSha256;
                byte[] keys = Util.GetBytes(TbFileKey.Text);
                hmacKey = keys.Take(32);
                aesKey = keys.Skip(32);
            }
            FTEventArgs args = new FTEventArgs(new Identifier(0), await FileMeta.FromFileAsync(path, algorithm, hmacKey, aesKey, null), path);
            args.Progress += VslClient_FTProgress;
            args.Finished += VslClient_FTFinished;
            if (!await vslClient.FileTransfer.StartUploadAsync(args)) return;
            btnReceiveFile.Enabled = false;
            btnSendFile.Enabled = false;
        }

        private async void BtnPenetrationTest_Click(object sender, EventArgs e)
        {
            if (pentest.Running)
            {
                pentest.Stop();
                btnPenetrationTest.Text = "Stresstest";
            }
            else
            {
                Task t = pentest.RunAsync(5000);
                btnPenetrationTest.Text = "Stoppen";
                await t;
                pentest.Stop();
                btnPenetrationTest.Text = "Stresstest";
                MessageBox.Show(string.Format("{0} attacks in {1}ms", pentest.Done, pentest.ElapsedTime));
            }
        }

        private void BtnCleanup_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Program.Clients.Cleanup();
            stopwatch.Stop();
            MessageBox.Show($"Cleanup successful after {stopwatch.ElapsedMilliseconds} ms.");
            GC.Collect();
        }

        private void VslClient_FTProgress(object sender, FTProgressEventArgs e)
        {
            PbFileTransfer.Value = Convert.ToInt32(e.Percentage * 100);
        }

        private void VslClient_FTFinished(object sender, EventArgs e)
        {
            FTEventArgs args = (FTEventArgs)sender;
            args.Progress -= VslClient_FTProgress;
            args.Finished -= VslClient_FTFinished;
            if (args.Mode == StreamMode.GetFile &&
                MessageBox.Show("Möchten Sie die empfangenen Metadaten übernehmen?",
                "Metadaten übernehmen?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                args.FileMeta.Apply(args.Path, Program.TempPath);
            btnReceiveFile.Enabled = true;
            btnSendFile.Enabled = true;
        }

        private async void VslClient_FTFileMetaReceived(object sender, EventArgs e)
        {
            FTEventArgs args = (FTEventArgs)sender;
            if (args.FileMeta.Algorithm == ContentAlgorithm.Aes256CbcHmacSha256 && !string.IsNullOrWhiteSpace(TbFileKey.Text))
            {
                byte[] keys = Util.GetBytes(TbFileKey.Text);
                args.FileMeta.Decrypt(keys.Take(32), keys.Skip(32));
            }
            await vslClient.FileTransfer.ContinueAsync(args);
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (vslClient != null)
            {
                vslClient.ConnectionClosed -= VSL_Close;
                if (clientConnected)
                    vslClient.CloseConnection("Closing VSLTest", null);
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

        private void LbFileKey_Click(object sender, EventArgs e)
        {
            byte[] buffer = new byte[64];
            using (var csp = new System.Security.Cryptography.RNGCryptoServiceProvider())
                csp.GetBytes(buffer);
            TbFileKey.Text = Util.ToHexString(buffer);
        }
    }
}