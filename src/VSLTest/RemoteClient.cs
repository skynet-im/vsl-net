using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSL;
using VSL.BinaryTools;
using VSL.FileTransfer;

namespace VSLTest
{
    public class RemoteClient : IVSLCallback
    {
        private VSLServer Vsl;
        private FileMeta lastMeta;

        public RemoteClient()
        {
            ImmutableInterlocked.Update(ref Program.Clients, x => x.Add(this));
        }

        public void OnInstanceCreated(VSLSocket socket)
        {
            Vsl = (VSLServer)socket;
            Vsl.FileTransfer.Request += Vsl_FileTransferRequested;
#if DEBUG
            Vsl.LogHandler = Program.Log;
#endif
        }

        public Task OnConnectionEstablished()
        {
            Program.Log(Vsl, "Client connected using protocol version " + Vsl.ConnectionVersionString);
            return Task.CompletedTask;
        }

        public Task OnPacketReceived(byte id, byte[] content)
        {
            if (content.Length > 1024)
                MessageBox.Show(string.Format("Server received: ID={0} Content={1}", id, content.Length));
            else
                MessageBox.Show(string.Format("Server received: ID={0} Content={1}", id, Util.ToHexString(content)));
            return Task.CompletedTask;
        }

        public void OnConnectionClosed(ConnectionCloseReason reason, string message, Exception exception)
        {
            Vsl.Dispose();
            ImmutableInterlocked.Update(ref Program.Clients, x => x.Remove(this));
            Interlocked.Increment(ref Program.Disconnects);
#if DEBUG
            Program.Log(Vsl, message);
#endif
        }


        public void SendPacket(byte id, byte[] content)
        {
            Vsl.SendPacketAsync(id, content);
        }

        public void CloseConnection(string message, Exception exception = null)
        {
            Vsl.CloseConnection(message, exception);
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