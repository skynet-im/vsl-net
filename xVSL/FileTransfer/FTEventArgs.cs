using System;
using System.Collections.Generic;
using System.IO;
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

        internal bool 

        internal bool OpenStream()
        {
            // TODO: Handle Exceptions at opening the FileStream
            if (Mode == StreamMode.GetHeader)
                throw new InvalidOperationException();
            else if (Mode == StreamMode.GetFile)
            {
                FileStream fs = new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.None);
                if (FileMeta.FileEncryption == ContentAlgorithm.None)
                    Stream = new ShaStream(fs, System.Security.Cryptography.CryptoStreamMode.Write);
                else if (FileMeta.FileEncryption == ContentAlgorithm.Aes256Cbc)
                    Stream = new AesShaStream(fs, FileMeta.FileKey, System.Security.Cryptography.CryptoStreamMode.Write, CryptographicOperation.Decrypt);
                else
                {
                    parent.ExceptionHandler.CloseConnection("InvalidFileAlgorithm",
                        "Cannot run file transfer with " + FileMeta.FileEncryption + ".\r\n\tat FTEventArgs.OpenStream()");
                    return false;
                }
            }
            else // Mode == StreamMode.UploadFile
            {
                FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (FileMeta.FileEncryption == ContentAlgorithm.None)
                    Stream = new ShaStream(fs, System.Security.Cryptography.CryptoStreamMode.Read);
                else if (FileMeta.FileEncryption == ContentAlgorithm.Aes256Cbc)
                    Stream = new AesShaStream(fs, FileMeta.FileKey, System.Security.Cryptography.CryptoStreamMode.Read, CryptographicOperation.Encrypt);
                else
                {
                    parent.ExceptionHandler.CloseConnection("InvalidFileAlgorithm",
                        "Cannot run file transfer with " + FileMeta.FileEncryption + ".\r\n\tat FTEventArgs.OpenStream()");
                    return false;
                }
            }
            return true;
        }

        internal void CloseStream(bool success)
        {
            Stream?.Close();
            Stream = null;
            if (success)
                OnFinished();
            else
                OnCanceled();
        }

        internal void OnFileMetaTransfered()
        {
            parent.ThreadManager.QueueWorkItem((ct) => Progress?.Invoke(this, new FTProgressEventArgs(0, FileMeta.Length)));
        }

        internal void OnFinished()
        {
            parent.ThreadManager.QueueWorkItem((ct) => Finished?.Invoke(this, null));
        }

        internal void OnCanceled()
        {
            parent.ThreadManager.QueueWorkItem((ct) => Canceled?.Invoke(this, null));
        }
    }
}