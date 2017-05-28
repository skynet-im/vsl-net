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
        /// <summary>
        /// Gets the ID for the requested file.
        /// </summary>
        public Identifier ID { get; }
        /// <summary>
        /// Gets the stream mode for the file transfer.
        /// </summary>
        public StreamMode Mode { get; }
    }
}