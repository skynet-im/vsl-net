using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.FileTransfer
{
    /// <summary>
    /// The server implementation of VSL file transfer.
    /// </summary>
    public class FileTransferServer : FileTransferSocket
    {
        // <fields
        new internal VSLServer parent;
        //  fields>
        // <constructor
        internal FileTransferServer(VSLServer parent)
        {
            this.parent = parent;
            base.parent = parent;
            InitializeComponent();
        }
        //  constructor>
        // <events
        /// <summary>
        /// The FileTransferRequested event occurs when the client has requested a VSL file transfer.
        /// </summary>
        public event EventHandler<FileTransferRequestedEventArgs> FileTransferRequested;
        /// <summary>
        /// Raises the OnFileTransferRequested event.
        /// </summary>
        internal void OnFileTransferRequested()
        {
            FileTransferRequested?.Invoke(this, new FileTransferRequestedEventArgs(this));
        }
        //  events>
        // <functions
        internal async void AcceptFileTransfer(string path)
        {
            if (string.IsNullOrEmpty(path))
                await parent.manager.SendPacketAsync(new Packet.P06Accepted(false, 7, Packet.ProblemCategory.None));
            else if (!File.Exists(path))
                throw new FileNotFoundException("The file for the VSL file transfer could not be found @" + path);
            else
            {

            }
        }
        //  functions>
    }
}