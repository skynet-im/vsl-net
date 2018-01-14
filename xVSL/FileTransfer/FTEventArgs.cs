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

        public FTEventArgs(FTSocket parent)
        {
            this.parent = parent;
        }

        public event EventHandler Canceled;
        public event EventHandler Finished;

        public FileMeta FileMeta { get; set; }
        public string Path { get; set; } // If not assigned, the downloads folder should be used automatically
        public byte[] Key { get; set; }
        public ContentAlgorithm HeaderAlgorithm { get; private set; }
        public ContentAlgorithm FileAlgorithm { get; private set; }
        internal byte[] FileKey { get; set; }
        internal byte[] FileIV { get; set; }

        internal void OnHeaderReceived()
        {

        }
    }
}