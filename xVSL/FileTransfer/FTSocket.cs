using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VSL.Crypt;

namespace VSL.FileTransfer
{
    public abstract class FTSocket
    {
        public event EventHandler<FTEventArgs> Request;

        public void Accept(FTEventArgs e)
        {
            //TODO: Handle any type of request.
        }

        public void Cancel(FTEventArgs e)
        {
            //TODO: Cancel request or file transfer
        }

        public void Download(FTEventArgs e)
        {
            //TODO: Send a request. Compare hashes of local and server file.
        }

        public void Upload(FTEventArgs e)
        {
            //TODO: Send a request. Upload header and file.
        }

        internal abstract void ReceiveHeader(FTEventArgs e);
        internal abstract void ReceiveFile(FTEventArgs e);
        internal abstract void SendHeader(FTEventArgs e);
        /// <summary>
        /// Sends only the raw file data.
        /// </summary>
        /// <param name="e">Work Item</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="InvalidDataException"/>
        internal protected virtual void SendFile(FTEventArgs e)
        {
            // Architecture has changed
            //if (string.IsNullOrWhiteSpace(e.Path))
            //    throw new ArgumentNullException("e.Path", "You must specify the path of the file");
            //if (!File.Exists(e.Path))
            //    throw new FileNotFoundException("You can only send existing files", e.Path);
            //e.FileStream = new FileStream(e.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            //if (e.FileAlgorithm != ContentAlgorithm.None)
            //{
            //    int first = e.FileStream.ReadByte();
            //    if (first <= 0 || (ContentAlgorithm)Convert.ToByte(first) == e.FileAlgorithm)
            //        throw new InvalidDataException("Algorithm is wrong or could not be found.");
            //}
            //byte[] buf = new byte[262144];
            //if (e.FileAlgorithm == ContentAlgorithm.Aes256Cbc && e.FileKey != null)
            //{
                
            //}
        }       
    }
}