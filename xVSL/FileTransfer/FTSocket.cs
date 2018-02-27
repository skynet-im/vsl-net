using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VSL.Crypt;
using VSL.Packet;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Provides basic functions for transfering files. Only transfer one file at once!
    /// </summary>
    public class FTSocket
    {
        private VSLSocket parent;
        private FTEventArgs currentItem;

        internal FTSocket(VSLSocket parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// Occurs when a file transfer request was received.
        /// </summary>
        public event EventHandler<FTEventArgs> Request;

        /// <summary>
        /// Accepts any type of request and starts the file transfer.
        /// </summary>
        /// <param name="e">The related request with many important information.</param>
        /// <param name="path">Local path where the file currently exists or will be stored.</param>
        public void Accept(FTEventArgs e, string path) => Accept(e, path, null);

        /// <summary>
        /// Accepts a DownloadHeader or DownloadFile request with custom metadata.
        /// </summary>
        /// <param name="e">The related request with many important information.</param>
        /// <param name="path">Local path where the file currently exists or will be stored.</param>
        /// <param name="meta">Only for file sending! The metadata to send especially for E2E server applications.</param>
        public void Accept(FTEventArgs e, string path, FileMeta meta)
        {
            e.Path = path;
            e.Assign(parent, this);
            e.FileMeta = meta;
            currentItem = e;

            parent.manager.SendPacket(new P06Accepted(true, 7, ProblemCategory.None)); // accept the transfer
            if (currentItem.Mode == StreamMode.PushHeader || currentItem.Mode == StreamMode.PushFile) // start by sending the FileMeta
            {
                if (currentItem.FileMeta == null)
                    currentItem.FileMeta = new FileMeta(path, ContentAlgorithm.None);
                parent.manager.SendPacket(new P08FileHeader(currentItem.FileMeta.GetBinaryData(parent.ConnectionVersion.Value)));
            }
            else if (meta != null)
            {
                parent.Logger.I("You have supplied a FileMeta for a receive operation. This FileMeta will be ignored.\r\n" +
                    "\tat FTSocket.Accept(FTEventArgs, String, FileMeta)");
            }
        }

        /// <summary>
        /// Cancels a pending request or a running file transfer.
        /// </summary>
        /// <param name="e"></param>
        public void Cancel(FTEventArgs e) // Client & Server
        {
            parent.manager.SendPacket(new P06Accepted(false, 7, ProblemCategory.None)); // this packet cancels any request or running transfer
            e.CloseStream(false);
            currentItem = null;
        }

        /// <summary>
        /// Requests the header of a remote file. All further information is specfied in the <see cref="FTEventArgs"/>.
        /// </summary>
        /// <param name="e">Specifies an identifier for the remote file, a local path and many more information of the file header to download.</param>
        public void DownloadHeader(FTEventArgs e) // Client
        {
            e.Assign(parent, this);
            e.Mode = StreamMode.GetHeader;
            currentItem = e;
            parent.manager.SendPacket(new P07OpenFileTransfer(e.Identifier, e.Mode));
        }

        /// <summary>
        /// Requests a remote file with its header. All further information is specfied in the <see cref="FTEventArgs"/>. After a <see cref="FileMeta"/> was received you can start the actual download with <see cref="Continue(FTEventArgs)"/>.
        /// </summary>
        /// <param name="e">Specifies an identifier for the remote file, a local path and many more information of the file to download.</param>
        public void Download(FTEventArgs e) // Client
        {
            e.Assign(parent, this);
            e.Mode = StreamMode.GetFile;
            currentItem = e;
            parent.manager.SendPacket(new P07OpenFileTransfer(e.Identifier, e.Mode));
        }

        /// <summary>
        /// Requests the permission to upload a file with its header. All further information is specfied in the <see cref="FTEventArgs"/>.
        /// </summary>
        /// <param name="e">Specifies an remote identifier for the file, a local path and many more information of the file to upload.</param>
        public void Upload(FTEventArgs e) // Client
        {
            e.Assign(parent, this);
            e.Mode = StreamMode.PushFile;
            currentItem = e;
            parent.manager.SendPacket(new P07OpenFileTransfer(e.Identifier, e.Mode));
        }

        /// <summary>
        /// Continues a file receive operation after a <see cref="FileMeta"/> was received. Cryptographic keys stored in this <see cref="FileMeta"/> will be used if available.
        /// </summary>
        /// <param name="e">The associated file transfer operation to continue.</param>
        /// <returns></returns>
        public bool Continue(FTEventArgs e)
        {
            if (e.Mode != StreamMode.GetFile)
            {
                parent.ExceptionHandler.CloseConnection("InvalidOperation",
                    $"You cannot continue a file transfer operation with {e.Mode}. Only file transfer with StreamMode.GetFile can be continued.\r\n" +
                    "\tat FTSocket.Continue(FTEventArgs)");
                return false;
            }
            else
            {
                if (!currentItem.OpenStream()) return false;
                return parent.manager.SendPacket(new P06Accepted(true, 8, ProblemCategory.None));
            }
        }

        internal bool OnPacketReceived(P06Accepted packet)
        {
            if (currentItem == null) // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "Cannot resume file transfer for the received accepted packet.\r\n" +
                    "\tat FTSocket.OnPacketReceived(P06Accepted)");
                return false;
            }
            if (!packet.Accepted && packet.RelatedPacket == 7) // Cancellation is always made by denying P07OpenFileTransfer
            {
                currentItem.CloseStream(false);
                currentItem = null;
            }
            else if (packet.Accepted && packet.RelatedPacket == 7)
            {
                if (currentItem.Mode == StreamMode.PushHeader || currentItem.Mode == StreamMode.PushFile)
                    parent.manager.SendPacket(new P08FileHeader(currentItem.FileMeta.GetBinaryData(parent.ConnectionVersion.Value)));
                // counterpart accepted file transfer -> we have to sent the FileMeta first

                // No exceptions here because every request type get's an accepted packet 
            }
            else if (packet.Accepted && packet.RelatedPacket == 8)
            {
                currentItem.OnFileMetaTransfered();
                if (currentItem.Mode == StreamMode.PushFile) // only StreamMode.PushFile wants to receive the file data
                {
                    if (!currentItem.OpenStream()) return false;
                    return SendBlock();
                }
                else if (currentItem.Mode != StreamMode.PushHeader)
                {
                    parent.ExceptionHandler.CloseConnection("InvalidPacket",
                        "The running file transfer is not supposed to receive an accepted packet for a file header.\r\n" +
                        "\tat FTSocket.OnPacketReceived(P06Accepted)" +
                        "\tpacket.RelatedPacket=" + packet.RelatedPacket);
                    return false;
                }
            }
            else if (packet.Accepted && packet.RelatedPacket == 9)
            {
                if (currentItem.Mode != StreamMode.PushFile)
                {
                    parent.ExceptionHandler.CloseConnection("InvalidPacket",
                        "The running file transfer is not supposed to receive an accepted packet for a file data block.\r\n" +
                        "\tat FTSocket.OnPacketReceived(P06Accepted)" +
                        "\tpacket.RelatedPacket=" + packet.RelatedPacket);
                    return false;
                }
                else if (currentItem.Stream != null)
                    return SendBlock();
                else
                    currentItem = null;
            }
            return true;
        }

        private bool SendBlock()
        {
            byte[] buffer = new byte[262144];
            ulong pos = Convert.ToUInt64(currentItem.Stream.Position);
            int count;
            try
            {
                count = currentItem.Stream.Read(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
            if (!parent.manager.SendPacket(new P09FileDataBlock(pos, Util.TakeBytes(buffer, count)))) return false;
            currentItem.OnProgress();
            if (count < buffer.Length)
                return currentItem.CloseStream(true);
            return true;
        }

        internal bool OnPacketReceived(P07OpenFileTransfer packet)
        {
            if (currentItem == null)
            {
                FTEventArgs e = new FTEventArgs(packet.Identifier, packet.StreamMode);
                currentItem = e;
                parent.ThreadManager.QueueWorkItem((ct) => Request?.Invoke(this, e));
                if (parent.Logger.InitD) parent.Logger.D($"FileTransfer with {e.Mode} and Identifier {e.Identifier} requested");
                return true;
            }
            else // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidRequest",
                    "A new file transfer was requested before the last one was finished or aborted.\r\n" +
                    "\tat FTSocket.OnPacketReceived(P07OpenFileTransfer)");
                return false;
            }
        }

        internal bool OnPacketReceived(P08FileHeader packet)
        {
            if (currentItem == null) // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "Cannot resume file transfer for the received file header.\r\n" +
                    "\tat FTSocket.OnPacketReceived(P08FileHeader)");
                return false;
            }
            if (currentItem.Mode != StreamMode.GetHeader && currentItem.Mode != StreamMode.GetFile)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "The running file transfer is not supposed to receive a file header.\r\n" +
                    "\tat FTSocket.OnPacketReceived(P08FileHeader)");
                return false;
            }
            if (currentItem.FileMeta != null)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "The running file transfer has already received a file header.\r\n" +
                    "\tat FTSocket.OnPacketReceived(P08FileHeader)");
                return false;
            }
            currentItem.FileMeta = new FileMeta(packet.BinaryData, parent.ConnectionVersion.Value);
            currentItem.OnFileMetaTransfered();
            if (parent.Logger.InitD) parent.Logger.D("Successfully received file meta.");
            if (currentItem.Mode == StreamMode.GetHeader)
            {
                currentItem.OnFinished();
                return parent.manager.SendPacket(new P06Accepted(true, 8, ProblemCategory.None));
            }
            // We do not answer for StreamMode.GetFile here, because this is done by FTSocket.Continue(FTEventArgs)
            // in order to give the opportunity to set keys.
            return true;
        }

        internal bool OnPacketReceived(P09FileDataBlock packet)
        {
            if (currentItem == null) // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "Cannot resume file transfer for the received file data block.\r\n" +
                    "\tat FTSocket.OnPacketReceived(P09FileDataBlock)");
                return false;
            }
            if (currentItem.Mode != StreamMode.GetFile)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "The running file transfer is not supposed to receive a file data block.\r\n" +
                    "\tat FTSocket.OnPacketReceived(P09FileDataBlock)\r\n" +
                    $"\tMode={currentItem.Mode}");
                return false;
            }
            if (currentItem.Stream == null)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "The request for the first FileDataBlock has not been sent yet.\r\n" +
                    "\tat FTSocket.OnPacketReceived(P09FileDataBlock)");
                return false;
            }
            try
            {
                currentItem.Stream.Write(packet.DataBlock, 0, packet.DataBlock.Length);
            }
            catch (Exception ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
            currentItem.OnProgress();
            if (currentItem.Stream.Position == currentItem.FileMeta.Length)
            {
                if (!currentItem.CloseStream(true)) return false;
                currentItem = null;
            }
            return parent.manager.SendPacket(new P06Accepted(true, 9, ProblemCategory.None));
        }
    }
}