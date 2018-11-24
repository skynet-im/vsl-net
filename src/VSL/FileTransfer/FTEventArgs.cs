using System;
using System.IO;
using System.Threading;
using VSL.BinaryTools;
using VSL.Crypt;
using VSL.Crypt.Streams;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Represents a running or upcoming file transfer and provides all required information. Use with <see cref="FTSocket"/>. Design inspired by <see cref="System.Net.Sockets.SocketAsyncEventArgs"/>.
    /// </summary>
    public class FTEventArgs : EventArgs, IDisposable
    {
        private VSLSocket parent;
        private FTSocket socket;
        private CancellationTokenSource cts;

        /// <summary>
        /// Initalizes a new instance of the <see cref="FTEventArgs"/> that can be used to send the associated file.
        /// </summary>
        /// <param name="identifier">A universal identifier to specify the file to process.</param>
        /// <param name="meta">Meta data of the file and required cryptographic keys.</param>
        /// <param name="path">The path where file currently exists or will be stored.</param>
        public FTEventArgs(Identifier identifier, FileMeta meta, string path)
            : this()
        {
            Identifier = identifier;
            FileMeta = meta;
            Path = path;
        }

        internal FTEventArgs(Identifier identifier, StreamMode mode)
            : this()
        {
            Identifier = identifier;
            Mode = mode;
        }

        private FTEventArgs()
        {
            cts = new CancellationTokenSource();
            CancellationToken = cts.Token;
        }

        /// <summary>
        /// Occurs when the file transfer is denied or canceled.
        /// </summary>
        public event EventHandler Canceled;
        /// <summary>
        /// Occurs when the file transfer was finished successfully.
        /// </summary>
        public event EventHandler Finished;
        /// <summary>
        /// Occurs when VSL achieved a progress running the file transfer.
        /// </summary>
        public event EventHandler<FTProgressEventArgs> Progress;
        /// <summary>
        /// Occurs when a <see cref="FileTransfer.FileMeta"/> was received. You may have to call <see cref="FTSocket.ContinueAsync(FTEventArgs)"/> in order to continue the file transfer.
        /// </summary>
        public event EventHandler FileMetaReceived;

        /// <summary>
        /// Gets the remote identifier for the transfered file.
        /// </summary>
        public Identifier Identifier { get; }
        /// <summary>
        /// Gets the operatening mode of this file transfer.
        /// </summary>
        public StreamMode Mode { get; internal set; }
        /// <summary>
        /// Gets the <see cref="FileTransfer.FileMeta"/> associated to this file. This class contains all meta data of the file including cryptographic keys.
        /// </summary>
        public FileMeta FileMeta { get; internal set; }
        /// <summary>
        /// Gets the local path of the current file transfer. From this location a file will be uploaded or stored when downloading.
        /// </summary>
        public string Path { get; internal set; }
        internal HashStream Stream { get; private set; }
        internal CancellationToken CancellationToken { get; }

        /// <summary>
        /// Assigns this <see cref="FTEventArgs"/> to a <see cref="FTSocket"/>. If it is already assigned, the method returns false.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="socket"></param>
        /// <returns>Whether the assignment was successful.</returns>
        /// <exception cref="ArgumentNullException"/>
        internal bool Assign(VSLSocket parent, FTSocket socket)
        {
            if (this.parent == null || this.socket == null)
            {
                this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
                this.socket = socket ?? throw new ArgumentNullException(nameof(socket));
                return true;
            }
            else return false;
        }

        internal bool OpenStream()
        {
            if (Mode == StreamMode.GetHeader || Mode == StreamMode.PushHeader)
            {
                parent.ExceptionHandler.CloseConnection("InvalidOperation",
                    $"You should not try to open a file stream with {Mode}",
                    nameof(FTEventArgs), nameof(OpenStream));
                return false;
            }
            FileStream fileStream;
            FileMode fileMode;
            FileAccess fileAccess;
            FileShare fileShare;
            System.Security.Cryptography.CryptoStreamMode streamMode;
            CryptographicOperation operation;
            if (Mode == StreamMode.GetFile)
            {
                fileMode = FileMode.Create;
                fileAccess = FileAccess.Write;
                fileShare = FileShare.None;
                streamMode = System.Security.Cryptography.CryptoStreamMode.Write;
                operation = CryptographicOperation.Decrypt;
            }
            else // Mode == StreamMode.PushFile
            {
                fileMode = FileMode.Open;
                fileAccess = FileAccess.Read;
                fileShare = FileShare.Read;
                streamMode = System.Security.Cryptography.CryptoStreamMode.Read;
                operation = CryptographicOperation.Encrypt;
            }
            try
            {
                fileStream = new FileStream(Path, fileMode, fileAccess, fileShare);
            }
            catch (Exception ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
            if (FileMeta.FileEncryption == ContentAlgorithm.None || !FileMeta.Available)
                Stream = new ShaStream(fileStream, streamMode);
            else if (FileMeta.FileEncryption == ContentAlgorithm.Aes256Cbc)
                Stream = new AesShaStream(fileStream, FileMeta.FileKey, streamMode, operation);
            else
            {
                parent.ExceptionHandler.CloseConnection("InvalidFileAlgorithm",
                    "Cannot run file transfer with " + FileMeta.FileEncryption,
                    nameof(FTEventArgs), nameof(OpenStream));
                return false;
            }
            return true;
        }

        internal bool Finish(bool success)
        {
            if (!disposedValue)
            {
                cts.Cancel();

                if (Stream == null) // For header requests and if transfer has not started
                {
                    RaiseAndDispose(success);
                    return true;
                }

                if (success && Mode == StreamMode.GetFile)
                {
                    success = FlushAndVerify();
                }

                try
                {
                    Stream.Dispose();
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    success = false;
                }

                RaiseAndDispose(success);
                return success;
            }
            else return false;
        }

        private void RaiseAndDispose(bool success)
        {
            if (success)
                OnFinished();
            else
                OnCanceled();

            cts.Dispose();
            disposedValue = true;
        }

        private bool FlushAndVerify()
        {
            try
            {
                if (!Stream.HasFlushedFinalBlock)
                    Stream.FlushFinalBlock();
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }

            if (FileMeta.Available && parent.ConnectionVersion.Value > 1 && !Stream.Hash.SafeEquals(FileMeta.SHA256))
            {
                // Do not check hash for VSL 1.1 because this version always sends an empty field.
                parent.ExceptionHandler.CloseConnection("FileCorrupted",
                    "The integrity checking resulted in a corrupted message. " +
                    $"Expected hash was {Util.ToHexString(FileMeta.SHA256)} " +
                    $"but the hash over the transfered data actually is {Util.ToHexString(Stream.Hash)}",
                    nameof(FTEventArgs));
                return false;
            }

            return true;
        }

        internal void OnFileMetaTransfered()
        {
            if (Mode == StreamMode.GetHeader || Mode == StreamMode.GetFile)
                parent.ThreadManager.Post(() => FileMetaReceived?.Invoke(this, null));
            var args = new FTProgressEventArgs(0, FileMeta.Length);
            parent.ThreadManager.Post(() => Progress?.Invoke(this, args));
        }

        internal void OnProgress()
        {
            var args = new FTProgressEventArgs(Stream.Position, FileMeta.Length);
            parent.ThreadManager.Post(() => Progress?.Invoke(this, args));
        }

        private void OnFinished()
        {
            parent.ThreadManager.Post(() => Finished?.Invoke(this, null));
#if DEBUG
            parent.Log($"Successfully transfered file with id {Identifier} and {Mode}{Environment.NewLine}" +
                $"to \"{Path}\" using ContentAlgorithm.{FileMeta.Algorithm}");
#endif
        }

        private void OnCanceled()
        {
            parent.ThreadManager.Post(() => Canceled?.Invoke(this, null));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Finish(success: false);
        }
        #endregion
    }
}