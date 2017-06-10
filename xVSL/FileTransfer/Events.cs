using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Event data when the file transfer progress has changed.
    /// </summary>
    public class FileTransferProgressEventArgs : EventArgs
    {
        internal FileTransferProgressEventArgs(long transfered, long total)
        {
            Percentage = total == 0 ? 1 : (double)transfered / total;
            TransferedBytes = transfered;
            TotalBytes = total;
        }
        /// <summary>
        /// Gets a value indicating the percentage of transfered to total bytes. The value reaches from 0 (header transfered) to 1 (download finished).
        /// </summary>
        public double Percentage { get; }
        /// <summary>
        /// Gets the amount of already transfered bytes.
        /// </summary>
        public long TransferedBytes { get; }
        /// <summary>
        /// Gets the total size of the transfered file.
        /// </summary>
        public long TotalBytes { get; }
    }
}