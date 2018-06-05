using System;
using System.Collections.Generic;
using System.Text;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Information about the current file transfer progress.
    /// </summary>
    public class FTProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Initalizes a new instance of the <see cref="FTProgressEventArgs"/> class.
        /// </summary>
        /// <param name="transfered"></param>
        /// <param name="total"></param>
        internal FTProgressEventArgs(long transfered, long total)
        {
            if (transfered < 0 && transfered != -1)
                throw new ArgumentOutOfRangeException("transfered");
            if (total < 0 && total != -1)
                throw new ArgumentOutOfRangeException("total");
            if ((transfered == -1 || total == -1) && transfered != total)
                throw new ArgumentException();
            TransferedBytes = transfered;
            TotalBytes = total;
            if (transfered == -1)
                Percentage = -1f;
            else
                Percentage = (float)transfered / total;
        }
        /// <summary>
        /// Gets the progress percentage of this file transfer. If the transfer has not started yet, the value is -1.
        /// </summary>
        public float Percentage { get; }
        /// <summary>
        /// Gets the count of transfered bytes. If the transfer has not started yet, the value is -1.
        /// </summary>
        public long TransferedBytes { get; }
        /// <summary>
        /// Gets the total length of the file in bytes. If the transfer has not started yet, the value is -1.
        /// </summary>
        public long TotalBytes { get; }
    }
}
