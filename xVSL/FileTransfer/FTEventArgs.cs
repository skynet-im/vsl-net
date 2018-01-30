using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VSL.Crypt;

namespace VSL.FileTransfer
{
    public class FTEventArgs : EventArgs
    {
        private FTSocket parent;
        internal FileStream FileStream;

        /// <summary>
        /// Initalizes a new instance of the <see cref="FTEventArgs"/> that can be used to send the associated file.
        /// </summary>
        /// <param name="parent">The underlying <see cref="FTSocket"/> to specify the VSL instance.</param>
        /// <param name="meta"></param>
        /// <param name="path"></param>
        public FTEventArgs(FTSocket parent, FileMeta meta, string path)
        {
            this.parent = parent;
            FileMeta = meta;
            Path = path;
        }

        public event EventHandler Canceled;
        public event EventHandler Finished;

        public FileMeta FileMeta { get; }
        public string Path { get; } // If not assigned, the downloads folder should be used automatically
        public ContentAlgorithm HeaderAlgorithm { get; private set; }
        public ContentAlgorithm FileAlgorithm { get; private set; }

        internal void OnHeaderReceived()
        {

        }
    }
}