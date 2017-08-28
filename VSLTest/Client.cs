using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSL;
using VSL.FileTransfer;

namespace VSLTest
{
    public class Client
    {
        public VSLServer Vsl;
        public Client(Socket native)
        {
            Vsl = new VSLServer(native, 0, 0, Program.Keypair, ThreadMgr.InvokeMode.ManagedThread, Vsl_ConnectionClosed);
            //Vsl.ConnectionClosed += Vsl_ConnectionClosed;
            Vsl.FileTransfer.FileTransferRequested += Vsl_FileTransferRequested;
            //Vsl.Logger.PrintDebugMessages = true;
            //Vsl.Logger.PrintExceptionMessages = true;
            //Vsl.Logger.PrintInfoMessages = true;
            Vsl.Logger.InvokeDebugMessages = false;
            Vsl.Logger.InvokeExceptionMessages = false;
            Vsl.Logger.InvokeInfoMessages = false;
        }

        private void Vsl_ConnectionClosed(object sender, ConnectionClosedEventArgs e)
        {
            //MessageBox.Show(string.Format("[Server] Connection closed\r\nReason: {0}\r\nReceived: {1}\r\nSent: {2}", e.Reason, e.ReceivedBytes, e.SentBytes));
            // TODO: Why is VSL null?
            Vsl?.Dispose();
            Program.Clients = Program.Clients.Remove(this);
#if DEBUG
            if (Program.Clients.Count == 0)
                Console.WriteLine("Empty");
#endif
        }

        private void Vsl_FileTransferRequested(object sender, EventArgs e)
        {
            if (Vsl.FileTransfer.Mode != StreamMode.UploadFile)
            {
                using (OpenFileDialog fd = new OpenFileDialog())
                {
                    fd.ShowDialog();
                    Vsl.FileTransfer.Path = fd.FileName;
                    Vsl.FileTransfer.Accept();
                }
            }
            else
            {
                Vsl.FileTransfer.Path = Path.Combine("D:", "ProgramData", "VSLTest", Path.GetRandomFileName());
                Vsl.FileTransfer.Accept();
            }
        }
    }
}