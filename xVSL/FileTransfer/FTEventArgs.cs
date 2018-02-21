using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VSL.Crypt;
using VSL.FileTransfer.Streams;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Represents a running or upcoming file transfer and provides all required information. Use with <see cref="FTSocket"/>. Design inspired by <see cref="System.Net.Sockets.SocketAsyncEventArgs"/>.
    /// </summary>
    public class FTEventArgs : EventArgs
    {
        private VSLSocket parent;
        private FTSocket socket;

        /// <summary>
        /// Initalizes a new instance of the <see cref="FTEventArgs"/> that can be used to send the associated file.
        /// </summary>
        /// <param name="identifier">A universal identifier to specify the file to process.</param>
        /// <param name="meta">Meta data of the file and required cryptographic keys.</param>
        /// <param name="path">The path where file currently exists or will be stored.</param>
        public FTEventArgs(Identifier identifier, FileMeta meta, string path)
        {
            Identifier = identifier;
            FileMeta = meta;
            Path = path;
        }

        internal FTEventArgs(Identifier identifier, StreamMode mode)
        {
            Identifier = identifier;
            Mode = mode;
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
        /// Occurs when a <see cref="FileTransfer.FileMeta"/> was received. You may have to call <see cref="FTSocket.Continue(FTEventArgs)"/> in order to continue the file transfer.
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

        internal void Assign(VSLSocket parent, FTSocket socket)
        {
            this.parent = parent;
            this.socket = socket;
        }

        internal bool OpenStream()
        {
            if (Mode == StreamMode.GetHeader || Mode == StreamMode.PushHeader)
            {
                parent.ExceptionHandler.CloseConnection("InvalidOperation",
                    $"You should not try to open a file stream with {Mode}\r\n" +
                    "\tat FTEventArgs.OpenStream()");
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
                    "Cannot run file transfer with " + FileMeta.FileEncryption + ".\r\n\tat FTEventArgs.OpenStream()");
                return false;
            }
            return true;
        }

        internal bool CloseStream(bool success)
        {
            try
            {
                Stream?.Dispose();
            }
            catch (Exception ex)
            {
                parent.ExceptionHandler.CloseConnection(ex);
                return false;
            }
            byte[] hash = Stream.Hash;
            Stream = null;
            if (success)
            {
                if (FileMeta.Available && !hash.SequenceEqual(FileMeta.SHA256))
                {
                    OnCanceled();
                    parent.ExceptionHandler.CloseConnection("FileCorrupted",
                        "The integrity checking resulted in a corrupted message.\r\n" +
                        "\tat FTEventArgs.CloseStream(bool)");
                    return false;
                }
                else
                    OnFinished();
            }
            else
                OnCanceled();
            return true;
        }

        internal void OnFileMetaTransfered()
        {
            if (Mode == StreamMode.GetHeader || Mode == StreamMode.GetFile)
                parent.ThreadManager.QueueWorkItem((ct) => FileMetaReceived?.Invoke(this, null));
            var args = new FTProgressEventArgs(0, FileMeta.Length);
            parent.ThreadManager.QueueWorkItem((ct) => Progress?.Invoke(this, args));
        }

        internal void OnProgress()
        {
            var args = new FTProgressEventArgs(Stream.Position, FileMeta.Length);
            parent.ThreadManager.QueueWorkItem((ct) => Progress?.Invoke(this, args));
        }

        internal void OnFinished()
        {
            parent.ThreadManager.QueueWorkItem((ct) => Finished?.Invoke(this, null));
            if (parent.Logger.InitD) parent.Logger.D($"Successfully transfered file with id {Identifier} and {Mode}\r\n" +
                $"to \"{Path}\" using ContentAlgorithm.{FileMeta.Algorithm}");
        }

        internal void OnCanceled()
        {
            parent.ThreadManager.QueueWorkItem((ct) => Canceled?.Invoke(this, null));
        }
    }
}