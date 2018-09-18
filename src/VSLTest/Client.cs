using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using VSL;
using VSL.BinaryTools;
using VSL.FileTransfer;

namespace VSLTest
{
    public class Client
    {
        private VSLServer Vsl;
        private FileMeta lastMeta;
        /// <summary>
        /// Creates a VSLServer.
        /// </summary>
        /// <param name="native"></param>
        public Client(VSLServer server)
        {
            Vsl = server;
            Vsl.ConnectionEstablished += Vsl_ConnectionEstablished;
            Vsl.PacketReceived += Vsl_PacketReceived;
            Vsl.ConnectionClosed += Vsl_ConnectionClosed;
            Vsl.FileTransfer.Request += Vsl_FileTransferRequested;
#if DEBUG
            Vsl.LogHandler = Program.Log;
#endif
            Program.Clients.Add(this);
        }

        public void SendPacket(byte id, byte[] content)
        {
            Vsl.SendPacketAsync(id, content);
        }

        public void CloseConnection(string reason, Exception ex)
        {
            Vsl.CloseConnection(reason, ex);
        }

        private void Vsl_ConnectionEstablished(object sender, EventArgs e)
        {
            Program.Log(Vsl, "Client connected using protocol version " + Vsl.ConnectionVersionString);
        }

        private void Vsl_PacketReceived(object sender, PacketReceivedEventArgs e)
        {
            if (e.Content.Length > 1024)
                MessageBox.Show(string.Format("Server received: ID={0} Content={1}", e.Id, e.Content.Length));
            else
                MessageBox.Show(string.Format("Server received: ID={0} Content={1}", e.Id, Util.ToHexString(e.Content)));
        }

        private void Vsl_ConnectionClosed(object sender, ConnectionClosedEventArgs e)
        {
            Vsl.Dispose();
            if (!Program.Clients.Remove(this))
                throw new Exception("Second ConnectionClosed event");
            Interlocked.Increment(ref Program.Disconnects);
#if DEBUG
            Program.Log((VSLServer)sender, e.Message);
#endif
        }

        private async void Vsl_FileTransferRequested(object sender, FTEventArgs e)
        {
            if (e.Mode == StreamMode.PushHeader || e.Mode == StreamMode.PushFile)
            {
                using (OpenFileDialog fd = new OpenFileDialog())
                {
                    fd.InitialDirectory = Program.TempPath;
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        if (lastMeta != null && MessageBox.Show("Sie können die Metadaten der letzen Dateiübertragung erneut verwenden",
                            "Metadaten wiederverwenden?", MessageBoxButtons.YesNo) == DialogResult.No)
                            lastMeta = null;

                        await Vsl.FileTransfer.AcceptAsync(e, fd.FileName, lastMeta);
                    }
                    else
                        await Vsl.FileTransfer.CancelAsync(e);
                }
            }
            else
            {
                e.FileMetaReceived += Vsl_FTFileMetaReceived;
                await Vsl.FileTransfer.AcceptAsync(e, Path.Combine(Program.TempPath, Path.GetRandomFileName()));
            }
        }

        private async void Vsl_FTFileMetaReceived(object sender, EventArgs e)
        {
            FTEventArgs args = (FTEventArgs)sender;
            lastMeta = args.FileMeta;
            await Vsl.FileTransfer.ContinueAsync(args);
            args.FileMetaReceived -= Vsl_FTFileMetaReceived;
        }
    }
}