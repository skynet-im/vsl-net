using System;
using System.Collections.Generic;
using System.Text;

namespace VSL.FileTransfer
{
    public class FileMeta
    {
        // TODO: ContentAlgorithm

        /// <summary>
        /// Gets whether plain data is available and the properties and fields can be used.
        /// </summary>
        public bool Available { get; }
        /// <summary>
        /// Gets encrypted or unencrypted binary data that can be restored.
        /// </summary>
        public byte[] BinaryData { get; }
    }
}