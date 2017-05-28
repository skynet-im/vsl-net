using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Event data when a file transfer has been requested.
    /// </summary>
    public class FileTransferRequestedEventArgs : EventArgs
    {
        private FileTransferServer parent;
        private string _path;

        internal FileTransferRequestedEventArgs(FileTransferServer parent)
        {
            this.parent = parent;
        }
        /// <summary>
        /// Gets the ID for the requested file.
        /// </summary>
        public Identifier ID { get; }
        /// <summary>
        /// Gets the stream mode for the file transfer.
        /// </summary>
        public StreamMode Mode { get; }
        /// <summary>
        /// Gets or sets the file path for the file transfer. If this request is not about to be handled, path is null or empty.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                parent.AcceptFileTransfer(value);    
            }
        }
    }
}