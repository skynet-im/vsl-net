using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VSL.Crypt;
using VSL.Packet;

namespace VSL.FileTransfer
{
    public class FTSocket
    {
        private VSLSocket parent;
        private FTEventArgs currentItem;

        internal FTSocket(VSLSocket parent)
        {
            this.parent = parent;
        }

        //TODO: Use sync lock to suppress multiple file transfers

        public event EventHandler<FTEventArgs> Request;

        public void Accept(FTEventArgs e, string path)
        {
            //TODO: Handle any type of request.
        }

        public void Cancel(FTEventArgs e)
        {
            //TODO: Cancel request or file transfer
        }

        public void DownloadHeader(FTEventArgs e)
        {
            //TODO: Send a request and download only header.
            e.Mode = StreamMode.GetHeader;
            parent.manager.SendPacket(new P07OpenFileTransfer(e.Identifier, e.Mode));
        }

        public void Download(FTEventArgs e)
        {
            //TODO: Send a request. Compare hashes of local and server file.
            e.Mode = StreamMode.GetFile;
            parent.manager.SendPacket(new P07OpenFileTransfer(e.Identifier, e.Mode));
        }

        public void Upload(FTEventArgs e)
        {
            e.Mode = StreamMode.UploadFile;
            parent.manager.SendPacket(new P07OpenFileTransfer(e.Identifier, e.Mode));
            //TODO: Upload header
            //TODO: Encrypt and upload file
        }

        internal bool OnPacketReceived(P06Accepted packet)
        {

        }

        internal bool OnPacketReceived(P07OpenFileTransfer packet)
        {
            if (currentItem == null)
            {
                FTEventArgs e = new FTEventArgs(packet.Identifier, packet.StreamMode);
                currentItem = e;
                parent.ThreadManager.QueueWorkItem((ct) => Request?.Invoke(this, e));
                return true;
            }
            else
            {
                parent.ExceptionHandler.CloseConnection("InvalidRequest",
                    "A new file transfer was request before the last one was finished or aborted.\r\n\tat FTSocket.OnPacketReceived(P07OpenFileTransfer)");
                return false;
            }
        }

        internal bool OnPacketReceived(P08FileHeader packet)
        {

        }

        internal bool OnPacketReceived(P09FileDataBlock packet)
        {

        }
    }
}