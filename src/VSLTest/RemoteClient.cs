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
        private VSLServer vsl;
        private FileMeta lastMeta;

        public RemoteClient()
        {
            ImmutableInterlocked.Update(ref Program.Clients, x => x.Add(this));
        }

        public void OnInstanceCreated(VSLSocket socket)
        {
            vsl = (VSLServer)socket;
            vsl.FileTransfer.Request += Vsl_FileTransferRequested;
#if DEBUG
            vsl.LogHandler = Program.Log;
#endif
        }

        public Task OnConnectionEstablished()
        {
            Program.Log(vsl, "Client connected using protocol version " + vsl.ConnectionVersionString);
            return Task.CompletedTask;
        }

        public Task OnPacketReceived(byte id, byte[] content)
        {
            //if (content.Length > 1024)
            //    MessageBox.Show(string.Format("Server received: ID={0} Content={1}", id, content.Length));
            //else
            //    MessageBox.Show(string.Format("Server received: ID={0} Content={1}", id, Util.ToHexString(content)));
            return Task.CompletedTask;
        }

        public void OnConnectionClosed(ConnectionCloseReason reason, string message, Exception exception)
        {
            vsl.Dispose();
            ImmutableInterlocked.Update(ref Program.Clients, x => x.Remove(this));
            Interlocked.Increment(ref Program.Disconnects);
#if DEBUG
            Program.Log(vsl, message);
#endif
        }


        public void SendPacket(byte id, byte[] content)
        {
            vsl.SendPacketAsync(id, content);
        }

        public void CloseConnection(string message, Exception exception = null)
        {
            vsl.CloseConnection(message, exception);
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