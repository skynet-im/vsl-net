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
            e.Assign(parent, this);
            e.Mode = StreamMode.GetHeader;
            parent.manager.SendPacket(new P07OpenFileTransfer(e.Identifier, e.Mode));
        }

        public void Download(FTEventArgs e)
        {
            //TODO: Send a request. Compare hashes of local and server file.
            e.Assign(parent, this);
            e.Mode = StreamMode.GetFile;
            parent.manager.SendPacket(new P07OpenFileTransfer(e.Identifier, e.Mode));
        }

        public void Upload(FTEventArgs e)
        {
            e.Assign(parent, this);
            e.Mode = StreamMode.UploadFile;
            parent.manager.SendPacket(new P07OpenFileTransfer(e.Identifier, e.Mode));
            //TODO: Upload header
            //TODO: Encrypt and upload file
        }

        internal bool OnPacketReceived(P06Accepted packet)
        {
            if (currentItem == null) // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "Cannot resume file transfer for the received accepted packet.\r\n\tat FTSocket.OnPacketReceived(P06Accepted)");
                return false;
            }
            throw new NotImplementedException();
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
            else // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidRequest",
                    "A new file transfer was requested before the last one was finished or aborted.\r\n\tat FTSocket.OnPacketReceived(P07OpenFileTransfer)");
                return false;
            }
        }

        internal bool OnPacketReceived(P08FileHeader packet)
        {
            if (currentItem == null) // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "Cannot resume file transfer for the received file header.\r\n\tat FTSocket.OnPacketReceived(P08FileHeader)");
                return false;
            }
            if (currentItem.Mode != StreamMode.GetHeader && currentItem.Mode != StreamMode.GetFile)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "The running file transfer is not supposed to receive a file header.\r\n\tat FTSocket.OnPacketReceived(P08FileHeader)");
                return false;
            }
            if (currentItem.FileMeta == null)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "The running file transfer has already received a file header.\r\n\tat FTSocket.OnPacketReceived(P08FileHeader)");
                return false;
            }
            currentItem.FileMeta = new FileMeta(packet.BinaryData, parent.ConnectionVersion.Value);
            if (currentItem.Mode == StreamMode.GetHeader)
            {
                currentItem.OnFinished();
                return parent.manager.SendPacket(new P06Accepted(true, 8, ProblemCategory.None));
            }
            else
            {
                // TODO: Open FileStream
                return parent.manager.SendPacket(new P06Accepted(true, 8, ProblemCategory.None));
            }
        }

        internal bool OnPacketReceived(P09FileDataBlock packet)
        {
            if (currentItem == null) // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "Cannot resume file transfer for the received file data block.\r\n\tat FTSocket.OnPacketReceived(P09FileDataBlock)");
                return false;
            }
            if (currentItem.Mode != StreamMode.GetFile)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "The running file transfer is not supposed to receive a file data block.\r\n\tat FTSocket.OnPacketReceived(P09FileDataBlock)");
                return false;
            }
            // TODO: Write data to FileStream
            throw new NotImplementedException();
        }
    }
}