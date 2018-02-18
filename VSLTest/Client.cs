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
        public VSLServer Vsl;
        public Client(Socket native, Dispatcher dispatcher)
        {
            Vsl = new VSLServer(native, 0, 0, Program.Keypair, ThreadManager.CreateManagedThread(dispatcher));
            Vsl.PacketReceived += Vsl_PacketReceived;
            Vsl.ConnectionClosed += Vsl_ConnectionClosed;
            Vsl.FileTransfer.Request += Vsl_FileTransferRequested;
            Vsl.Logger.PrintDebugMessages = true;
            Vsl.Logger.PrintExceptionMessages = true;
            Vsl.Logger.PrintInfoMessages = true;
            Vsl.Logger.PrintUncaughtExceptions = true;
            Vsl.Logger.InvokeDebugMessages = false;
            Vsl.Logger.InvokeExceptionMessages = false;
            Vsl.Logger.InvokeInfoMessages = false;
            Program.Clients.Add(this);
            Vsl.Start();
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
                    fd.ShowDialog();
                    Vsl.FileTransfer.Accept(e, fd.FileName);
                }
            }
            else
                Vsl.FileTransfer.Accept(e, Path.Combine("D:", "ProgramData", "VSLTest", Path.GetRandomFileName()));
        }
    }
}