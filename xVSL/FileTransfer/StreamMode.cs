using System;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Specifies a stream mode for VSL file transfer.
    /// </summary>
    public enum StreamMode
    {
        /// <summary>
        /// Downloads only the file header with important meta data.
        /// </summary>
        GetHeader,
        /// <summary>
        /// Downloads the complete file.
        /// </summary>
        GetFile,
        /// <summary>
        /// Uploads only the file header with important meta data.
        /// </summary>
        PushHeader,
        /// <summary>
        /// Uploads the complete file.
        /// </summary>
        PushFile
    }
}