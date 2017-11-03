using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

namespace VSL.FileTransfer
{
    /// <summary>
    /// The client implementation of VSL file transfer.
    /// </summary>
    public class FileTransferClient : FileTransferSocket
    {
        // <fields
        new internal VSLClient parent;
        private bool receivingHeader = false;
        //  fields>
        // <constructor
        internal FileTransferClient(VSLClient parent)
        {
            this.parent = parent;
            base.parent = parent;
            InitializeComponent();
        }
        //  constructor>
        // <events
        /// <summary>
        /// The FileTransferDenied event occurs when the server has denied a pendig file transfer request.
        /// </summary>
        public event EventHandler FileTransferDenied;
        /// <summary>
        /// Raises the OnFileTransferDenied event.
        /// </summary>
        internal void OnFileTransferDenied()
        {
            parent.EventThread.QueueWorkItem((ct) => FileTransferDenied?.Invoke(this, new EventArgs()));
            Reset();
        }
        //  events>
        // <functions
        /// <summary>
        /// Sends a VSL file transfer request to the server.
        /// </summary>
        /// <param name="id">Identifier for the file to request.</param>
        /// <param name="mode">StreamMode for the VSL file request.</param>
        public async void RequestFile(Identifier id, StreamMode mode)
        {
            Task t = Task.Run(() => parent.manager.SendPacket(new P07OpenFileTransfer(id, mode)));
            ID = id;
            Mode = mode;
            await t;
        }
        internal override void OnAccepted(P06Accepted p)
        {
            if (p.RelatedPacket == 7)
            {
                if (p.Accepted)
                    ResumeRequest();
                else
                    OnFileTransferDenied();
            }
            else
                base.OnAccepted(p);
        }
        internal override void OnHeaderReceived(P08FileHeader packet)
        {
            if (receivingHeader)
            {
                SetHeaderPacket(Path, packet);
                OnFileTransferFinished();
                Reset();
            }
            else
                base.OnHeaderReceived(packet);
        }
        internal void ResumeRequest()
        {
            if (parent.Logger.InitD)
                parent.Logger.D("Starting file transfer with Mode " + Mode.ToString());
            switch (Mode)
            {
                case StreamMode.GetHeader:
                    ReceiveHeader();
                    break;
                case StreamMode.GetFile:
                    ReceiveFile();
                    break;
                case StreamMode.UploadFile:
                    SendFile();
                    break;
                default:
                    throw new NotImplementedException("Unknown StreamMode");
            }
        }
        internal void ReceiveHeader()
        {
            ReceivingFile = true;
            receivingHeader = true;
        }
        internal override void Reset()
        {
            base.Reset();
            receivingHeader = false;
        }
        //  functions>
    }
}