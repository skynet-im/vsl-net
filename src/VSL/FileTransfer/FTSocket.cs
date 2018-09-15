using System;
using System.Threading.Tasks;
using VSL.BinaryTools;
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
        public Task<bool> AcceptAsync(FTEventArgs e, string path) => AcceptAsync(e, path, null);

        /// <summary>
        /// Accepts a DownloadHeader or DownloadFile request with custom metadata.
        /// </summary>
        /// <param name="e">The related request with many important information.</param>
        /// <param name="path">Local path where the file currently exists or will be stored.</param>
        /// <param name="meta">Only for file sending! The metadata to send especially for E2E server applications.</param>
        public async Task<bool> AcceptAsync(FTEventArgs e, string path, FileMeta meta)
        {
            e.Path = path;
            e.Assign(parent, this);
            e.FileMeta = meta;
            currentItem = e;

            if (!await parent.Manager.SendPacketAsync(new P06Accepted(true, 7, ProblemCategory.None))) // accept the transfer
                return false;
            if (currentItem.Mode == StreamMode.PushHeader || currentItem.Mode == StreamMode.PushFile) // start by sending the FileMeta
            {
                if (currentItem.FileMeta == null)
                    currentItem.FileMeta = await FileMeta.FromFileAsync(path, ContentAlgorithm.None);
                if (!await parent.Manager.SendPacketAsync(new P08FileHeader(currentItem.FileMeta.GetBinaryData(parent.ConnectionVersion.Value))))
                    return false;
            }
            else if (meta != null)
            {
                throw new ArgumentException("You must not supply a FileMeta for a receive operation.", nameof(meta));
            }
            return true;
        }

        /// <summary>
        /// Cancels a pending request or a running file transfer.
        /// </summary>
        /// <param name="e"></param>
        public Task<bool> CancelAsync(FTEventArgs e)
        {
            Task<bool> t = parent.Manager.SendPacketAsync(new P06Accepted(false, 7, ProblemCategory.None)); // This packet cancels any request or running transfer.
            e.Assign(parent, this); // This assignment is necessary for FTEventArgs to print exceptions and raise events.
            e.CloseStream(false);
            currentItem = null;
            return t;
        }

        /// <summary>
        /// Requests the header of a remote file. All further information is specfied in the <see cref="FTEventArgs"/>.
        /// </summary>
        /// <param name="e">Specifies an identifier for the remote file, a local path and many more information of the file header to download.</param>
        public Task<bool> StartDownloadHeaderAsync(FTEventArgs e)
        {
            e.Assign(parent, this);
            e.Mode = StreamMode.GetHeader;
            currentItem = e;
            return parent.Manager.SendPacketAsync(new P07OpenFileTransfer(e.Identifier, e.Mode));
        }

        /// <summary>
        /// Requests a remote file with its header. All further information is specfied in the <see cref="FTEventArgs"/>. After a <see cref="FileMeta"/> was received you can start the actual download with <see cref="ContinueAsync(FTEventArgs)"/>.
        /// </summary>
        /// <param name="e">Specifies an identifier for the remote file, a local path and many more information of the file to download.</param>
        public Task<bool> StartDownloadAsync(FTEventArgs e)
        {
            e.Assign(parent, this);
            e.Mode = StreamMode.GetFile;
            currentItem = e;
            return parent.Manager.SendPacketAsync(new P07OpenFileTransfer(e.Identifier, e.Mode));
        }

        /// <summary>
        /// Requests the permission to upload a file with its header. All further information is specfied in the <see cref="FTEventArgs"/>.
        /// </summary>
        /// <param name="e">Specifies an remote identifier for the file, a local path and many more information of the file to upload.</param>
        public Task<bool> StartUploadAsync(FTEventArgs e)
        {
            e.Assign(parent, this);
            e.Mode = StreamMode.PushFile;
            currentItem = e;
            return parent.Manager.SendPacketAsync(new P07OpenFileTransfer(e.Identifier, e.Mode));
        }

        /// <summary>
        /// Continues a file receive operation after a <see cref="FileMeta"/> was received. Cryptographic keys stored in this <see cref="FileMeta"/> will be used if available.
        /// </summary>
        /// <param name="e">The associated file transfer operation to continue.</param>
        /// <returns>Returns true when the continuation succeeded.</returns>
        /// <exception cref="InvalidOperationException">When attempting to continue operations other than <see cref="StreamMode.GetFile"/>.</exception>
        public async Task<bool> ContinueAsync(FTEventArgs e)
        {
            if (e.Mode != StreamMode.GetFile)
            {
                throw new InvalidOperationException($"You cannot continue a file transfer operation with {e.Mode}. " +
                    "Only file transfer with StreamMode.GetFile can be continued.");
            }
            else
            {
                if (!currentItem.OpenStream()) return false;
                return await parent.Manager.SendPacketAsync(new P06Accepted(true, 8, ProblemCategory.None));
            }
        }

        internal async Task<bool> OnPacketReceivedAsync(P06Accepted packet)
        {
            const string name = nameof(OnPacketReceivedAsync) + "(" + nameof(P06Accepted) + "(";

            if (currentItem == null) // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "Cannot resume file transfer for the received accepted packet.",
                    nameof(FTSocket), name);
                return false;
            }
            if (!packet.Accepted && packet.RelatedPacket == 7) // Cancellation is always made by denying P07OpenFileTransfer
            {
                currentItem.CloseStream(false);
                currentItem = null;
            }
            else if (packet.Accepted && packet.RelatedPacket == 7)
            {
                if ((currentItem.Mode == StreamMode.PushHeader || currentItem.Mode == StreamMode.PushFile) &&
                    !await parent.Manager.SendPacketAsync(new P08FileHeader(currentItem.FileMeta.GetBinaryData(parent.ConnectionVersion.Value))))
                    return false;
                // counterpart accepted file transfer -> we have to sent the FileMeta first

                // No exceptions here because every request type get's an accepted packet 
            }
            else if (packet.Accepted && packet.RelatedPacket == 8)
            {
                currentItem.OnFileMetaTransfered();
                if (currentItem.Mode == StreamMode.PushFile) // only StreamMode.PushFile wants to receive the file data
                {
                    if (!currentItem.OpenStream()) return false;
                    return await SendBlockAsync();
                }
                else if (currentItem.Mode != StreamMode.PushHeader)
                {
                    parent.ExceptionHandler.CloseConnection("InvalidPacket",
                        "The running file transfer is not supposed to receive an accepted packet for a file header.",
                        nameof(FTSocket), name);
                    return false;
                }
            }
            else if (packet.Accepted && packet.RelatedPacket == 9)
            {
                if (currentItem.Mode != StreamMode.PushFile)
                {
                    parent.ExceptionHandler.CloseConnection("InvalidPacket",
                        "The running file transfer is not supposed to receive an accepted packet for a file data block.",
                        nameof(FTSocket), name);
                    return false;
                }
                else if (currentItem.Stream != null)
                    return await SendBlockAsync();
                else
                    currentItem = null;
            }
            return true;
        }

        private async Task<bool> SendBlockAsync()
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
            if (!await parent.Manager.SendPacketAsync(new P09FileDataBlock(pos, buffer.Take(count)))) return false;
            currentItem.OnProgress();
            if (count < buffer.Length)
                return currentItem.CloseStream(true);
            return true;
        }

        internal Task<bool> OnPacketReceivedAsync(P07OpenFileTransfer packet)
        {
            if (currentItem == null)
            {
                FTEventArgs e = new FTEventArgs(packet.Identifier, packet.StreamMode);
                currentItem = e;
                parent.ThreadManager.Post(() => Request?.Invoke(this, e));
#if DEBUG
                parent.Log($"FileTransfer with {e.Mode} and Identifier {e.Identifier} requested");
#endif
                return Task.FromResult(true);
            }
            else // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidRequest",
                    "A new file transfer was requested before the last one was finished or aborted.",
                    nameof(FTSocket), "OnPacketReceivedAsync(P07OpenFileTransfer)");
                return Task.FromResult(false);
            }
        }

        internal async Task<bool> OnPacketReceivedAsync(P08FileHeader packet)
        {
            const string name = nameof(OnPacketReceivedAsync) + "(" + nameof(P08FileHeader) + "(";

            if (currentItem == null) // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "Cannot resume file transfer for the received file header.",
                    nameof(FTSocket), name);
                return false;
            }
            if (currentItem.Mode != StreamMode.GetHeader && currentItem.Mode != StreamMode.GetFile)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "The running file transfer is not supposed to receive a file header.",
                    nameof(FTSocket), name);
                return false;
            }
            if (currentItem.FileMeta != null)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "The running file transfer has already received a file header.",
                    nameof(FTSocket), name);
                return false;
            }
            currentItem.FileMeta = new FileMeta(packet.BinaryData, parent.ConnectionVersion.Value);
            currentItem.OnFileMetaTransfered();
#if DEBUG
            parent.Log("Successfully received file meta.");
#endif
            if (currentItem.Mode == StreamMode.GetHeader)
            {
                currentItem.OnFinished();
                return await parent.Manager.SendPacketAsync(new P06Accepted(true, 8, ProblemCategory.None));
            }
            // We do not answer for StreamMode.GetFile here, because this is done by FTSocket.Continue(FTEventArgs)
            // in order to give the opportunity to set keys.
            return true;
        }

        internal async Task<bool> OnPacketReceivedAsync(P09FileDataBlock packet)
        {
            const string name = nameof(OnPacketReceivedAsync) + "(" + nameof(P09FileDataBlock) + ")";

            if (currentItem == null) // It may be more efficient not to close the connection but only to cancel the file transfer.
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "Cannot resume file transfer for the received file data block.",
                    nameof(FTSocket), name);
                return false;
            }
            if (currentItem.Mode != StreamMode.GetFile)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    $"The running file transfer with mode {currentItem.Mode} is not supposed to receive a file data block.",
                    nameof(FTSocket), name);
                return false;
            }
            if (currentItem.Stream == null)
            {
                parent.ExceptionHandler.CloseConnection("InvalidPacket",
                    "The request for the first FileDataBlock has not been sent yet.",
                    nameof(FTSocket), name);
                return false;
            }
            try
            {
                await currentItem.Stream.WriteAsync(packet.DataBlock, 0, packet.DataBlock.Length);
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
            return await parent.Manager.SendPacketAsync(new P06Accepted(true, 9, ProblemCategory.None));
        }
    }
}