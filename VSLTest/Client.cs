using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using VSL;
using VSL.FileTransfer;

namespace VSLTest
{
    public class Client
    {
        private VSLServer Vsl;
        private FileMeta lastMeta;
        /// <summary>
        /// Creates a VSLServer with <see cref="VSL.Threading.AsyncMode.ManagedThread"/>. If no <see cref="Dispatcher"/> is provided <see cref="VSL.Threading.AsyncMode.ThreadPool"/> is used.
        /// </summary>
        /// <param name="native"></param>
        /// <param name="dispatcher"></param>
        public Client(Socket native, Dispatcher dispatcher)
        {
            ThreadManager thrmgr;
            if (dispatcher == null)
                thrmgr = ThreadManager.CreateThreadPool();
            else
                thrmgr = ThreadManager.CreateManagedThread(dispatcher);
            Vsl = new VSLServer(native, 0, 0, Program.Keypair, thrmgr);
            Vsl.PacketReceived += Vsl_PacketReceived;
            Vsl.ConnectionClosed += Vsl_ConnectionClosed;
            Vsl.FileTransfer.Request += Vsl_FileTransferRequested;
            Vsl.CatchApplicationExceptions = false;
            Vsl.Logger.PrintDebugMessages = true;
            Vsl.Logger.PrintExceptionMessages = true;
            Vsl.Logger.PrintInfoMessages = true;
            Vsl.Logger.PrintUncaughtExceptions = true;
            Program.Clients.Add(this);
            Vsl.Start();
        }

        public void SendPacket(byte id, byte[] content)
        {
            Vsl.SendPacket(id, content);
        }

        public void CloseConnection(string reason)
        {
            Vsl.CloseConnection(reason);
        }

        private void Vsl_PacketReceived(object sender, PacketReceivedEventArgs e)
        {
            if (e.Content.Length > 1024)
                MessageBox.Show(string.Format("Server received: ID={0} Content={1}", e.ID, e.Content.Length));
            else
                MessageBox.Show(string.Format("Server received: ID={0} Content={1}", e.ID, VSL.Crypt.Util.ToHexString(e.Content)));
        }

        private void Vsl_ConnectionClosed(object sender, ConnectionClosedEventArgs e)
        {
            Vsl.Dispose();
            if (!Program.Clients.Remove(this))
                throw new Exception("Second ConnectionClosed event");
            Interlocked.Increment(ref Program.Disconnects);
        }

        private void Vsl_FileTransferRequested(object sender, FTEventArgs e)
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

                        Vsl.FileTransfer.Accept(e, fd.FileName, lastMeta);
                    }
                    else
                        Vsl.FileTransfer.Cancel(e);
                }
            }
            else
            {
                e.FileMetaReceived += Vsl_FTFileMetaReceived;
                Vsl.FileTransfer.Accept(e, Path.Combine(Program.TempPath, Path.GetRandomFileName()));
            }
        }

        private void Vsl_FTFileMetaReceived(object sender, EventArgs e)
        {
            FTEventArgs args = (FTEventArgs)sender;
            lastMeta = args.FileMeta;
            Vsl.FileTransfer.Continue(args);
            args.FileMetaReceived -= Vsl_FTFileMetaReceived;
        }
    }
}