using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

namespace VSL.FileTransfer
{
    /// <summary>
    /// The base class for file transfer implementations.
    /// </summary>
    public abstract class FileTransferSocket : IDisposable
    {
        // <fields
        internal VSLSocket parent;
        internal bool ReceivingFile = false;
        internal bool SendingFile = false;
        private Stopwatch stopwatch;
        /// <summary>
        /// The Identifier that is used to identify the file to transfer.
        /// </summary>
        public Identifier ID;
        /// <summary>
        /// The StreamMode that is being used by the current transfer.
        /// </summary>
        public StreamMode Mode;
        /// <summary>
        /// The local file path were the file is read or written.
        /// </summary>
        public string Path;
        private FileStream stream;
        private long transfered;
        private long length;
        private P08FileHeader header;
        //  fields>
        // <constructor
        internal void InitializeComponent()
        {

        }
        //  constructor>
        // <events
        /// <summary>
        /// The FileTransferFinished event occurs when the VSL file transfer has been finished.
        /// </summary>
        public event EventHandler FileTransferFinished;
        /// <summary>
        /// Raises the FileTransferFinished event.
        /// </summary>
        internal void OnFileTransferFinished()
        {
            parent.EventThread.QueueWorkItem((ct) => FileTransferFinished?.Invoke(this, new EventArgs()));
        }
        /// <summary>
        /// The FileTransferProgress event occurs when the progress of a running file transfer has changed.
        /// </summary>
        public event EventHandler<FileTransferProgressEventArgs> FileTransferProgress;
        /// <summary>
        /// Raises the FileTransferProgressEvent.
        /// </summary>
        internal void OnFileTransferProgress()
        {
            FileTransferProgressEventArgs args = new FileTransferProgressEventArgs(transfered, Mode != StreamMode.GetHeader ? length : 0);
            parent.EventThread.QueueWorkItem((ct) => FileTransferProgress?.Invoke(this, args));
        }
        //  events>
        // <functions
        internal P08FileHeader GetHeaderPacket(string path)
        {
            FileInfo fi = new FileInfo(path);
            return new P08FileHeader(fi.Name, Convert.ToUInt64(fi.Length), fi.Attributes, fi.CreationTime, fi.LastAccessTime, fi.LastWriteTime, new byte[0], new byte[0]);
        }
        internal void SetHeaderPacket(string path, P08FileHeader packet)
        {
            string newPath = "";
            try
            {
                newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), packet.Name);
                File.Move(path, newPath);
                FileInfo fi = new FileInfo(newPath)
                {
                    Attributes = packet.Attributes,
                    CreationTime = packet.CreationTime,
                    LastAccessTime = packet.LastAccessTime,
                    LastWriteTime = packet.LastWriteTime
                };
            }
            //catch (IOException ex)
            //{
            //    Console.WriteLine(string.Format("Source={0}, Target={1}, Exception{2}", path, newPath, ex));
            //}
            catch (Exception ex)
            {
                parent.ExceptionHandler.PrintException(ex);
            }
        }
        #region receive
        internal void ReceiveFile()
        {
            ReceivingFile = true;
            stream = new FileStream(Path, FileMode.Create);
        }
        internal virtual async void OnHeaderReceived(P08FileHeader packet)
        {
            Task t = parent.manager.SendPacketAsync(new P06Accepted(true, 8, ProblemCategory.None));
            length = Convert.ToInt64(packet.Length);
            header = packet;
            OnFileTransferProgress();
            await t;
        }
        internal async void OnDataBlockReceived(P09FileDataBlock packet)
        {
            if (stream == null && parent.Logger.InitD)
                parent.Logger.D("Waiting for stream to be initialized");
            while (stream == null)
            {
                await Task.Delay(100);
            }
            if (parent.Logger.InitD)
                parent.Logger.D("Stream initialized");
            Task r = parent.manager.SendPacketAsync(new P06Accepted(true, 9, ProblemCategory.None));
            Task w = stream.WriteAsync(packet.DataBlock, 0, packet.DataBlock.Length);
            OnFileTransferProgress();
            await r;
            await w;
            transfered += packet.DataBlock.Length;
            if (transfered == length)
            {
                stream.Close();
                SetHeaderPacket(Path, header);
                ReceivingFile = false;
                OnFileTransferFinished();
                Reset();
            }
        }
        #endregion
        #region send
        internal async void SendFile()
        {
            SendingFile = true;
            header = GetHeaderPacket(Path);
            length = Convert.ToInt64(header.Length);
            Task t = parent.manager.SendPacketAsync(header);
            try
            {
                stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                parent.ExceptionHandler.CancelFileTransfer(ex);
            }
            OnFileTransferProgress();
            stopwatch = new Stopwatch();
            await t;
        }
        internal virtual async void OnAccepted(P06Accepted p)
        {
            if (SendingFile)
            {
                if (stopwatch.IsRunning)
                    stopwatch.Stop();
                int elapsed = Convert.ToInt32(stopwatch.ElapsedMilliseconds);
                int size = 131072;
                if (elapsed > 0)
                {
                    if (elapsed < 100)
                        size = 1048568; // 8 bytes packet overhead
                    else if (elapsed < 500)
                        size = 524288;
                    else if (elapsed < 1000)
                        size = 262144;
                }
                byte[] buf = new byte[size];
                ulong startPos = Convert.ToUInt64(stream.Position);
                int count = await stream.ReadAsync(buf, 0, size);
                if (count == 0)
                {
                    stream.Close();
                    OnFileTransferFinished();
                    Reset();
                }
                else
                {
                    Task t = parent.manager.SendPacketAsync(new P09FileDataBlock(startPos, Crypt.Util.TakeBytes(buf, count)));
                    transfered += count;
                    OnFileTransferProgress();
                    await t;
                    stopwatch.Restart();
                }
            }
        }
        #endregion
        internal async void Cancel()
        {
            if (!ReceivingFile)
                throw new InvalidOperationException("You can not cancel a not existing file transfer");
            await parent.manager.SendPacketAsync(new P06Accepted(false, 7, ProblemCategory.None));
            Reset();
        }
        internal virtual void Reset()
        {
            ReceivingFile = false;
            SendingFile = false;
            ID = null;
            Path = null;
            stream = null;
            transfered = 0;
            length = 0;
            header = null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Specify whether managed objects should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // -TODO: dispose managed state (managed objects).
                    stream?.Dispose();
                }

                // -TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // -TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // -TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FileTransferSocket() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // -TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        //  functions>
    }
}