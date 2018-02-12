using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VSL.Crypt;

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

        public Identifier Identifier { get; }
        public StreamMode Mode { get; internal set; }
        private FileMeta _fileMeta;
        public FileMeta FileMeta
        {
            get => _fileMeta;
            internal set
            {
                _fileMeta = value;
                parent.ThreadManager.QueueWorkItem((ct) => Progress?.Invoke(this, new FTProgressEventArgs(0, _fileMeta.Length)));
            }
        }
        public string Path { get; }
        public ContentAlgorithm HeaderAlgorithm { get; private set; }
        public ContentAlgorithm FileAlgorithm { get; private set; }

        internal void Assign(VSLSocket parent, FTSocket socket)
        {
            this.parent = parent;
            this.socket = socket;
        }

        internal void OnFinished()
        {
            parent.ThreadManager.QueueWorkItem((ct) => Finished?.Invoke(this, null));
        }
    }
}