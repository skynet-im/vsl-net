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
    public class RemoteClient : VSL.Common.RemoteClient
    {
        private FileMeta lastMeta;

        public override void OnInstanceCreated(VSLSocket socket)
        {
            base.OnInstanceCreated(socket);

            vsl.FileTransfer.Request += Vsl_FileTransferRequested;
#if DEBUG
            vsl.LogHandler = Program.Log;
#endif
        }

        public override Task OnConnectionEstablished()
        {
            Program.Log(vsl, "Client connected using protocol version " + vsl.ConnectionVersionString);
            return Task.CompletedTask;
        }

        public override Task OnPacketReceived(byte id, byte[] content)
        {
            //if (content.Length > 1024)
            //    MessageBox.Show(string.Format("Server received: ID={0} Content={1}", id, content.Length));
            //else
            //    MessageBox.Show(string.Format("Server received: ID={0} Content={1}", id, Util.ToHexString(content)));
            return Task.CompletedTask;
        }

        public override void OnConnectionClosed(ConnectionCloseReason reason, string message, Exception exception)
        {
            base.OnConnectionClosed(reason, message, exception);

            Interlocked.Increment(ref Program.Disconnects);
#if DEBUG
            Program.Log(vsl, message);
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

                        await vsl.FileTransfer.AcceptAsync(e, fd.FileName, lastMeta);
                    }
                    else
                        await vsl.FileTransfer.CancelAsync(e);
                }
            }
            else
            {
                e.FileMetaReceived += Vsl_FTFileMetaReceived;
                await vsl.FileTransfer.AcceptAsync(e, Path.Combine(Program.TempPath, Path.GetRandomFileName()));
            }
        }

        private async void Vsl_FTFileMetaReceived(object sender, EventArgs e)
        {
            FTEventArgs args = (FTEventArgs)sender;
            lastMeta = args.FileMeta;
            await vsl.FileTransfer.ContinueAsync(args);
            args.FileMetaReceived -= Vsl_FTFileMetaReceived;
        }
    }
}