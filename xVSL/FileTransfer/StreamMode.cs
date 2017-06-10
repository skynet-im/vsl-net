using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Specifies a stream mode for VSL file transfer.
    /// </summary>
    public enum StreamMode : byte
    {
        /// <summary>
        /// Downloads only the file header with thumbnail and hash.
        /// </summary>
        GetHeader,
        /// <summary>
        /// Downloads the complete file.
        /// </summary>
        GetFile,
        /// <summary>
        /// Uploads the new or changed file.
        /// </summary>
        UploadFile
    }
}