﻿using System;
using System.Collections.Generic;
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
    public abstract class FileTransferSocket
    {
        // <fields
        internal VSLSocket parent;
        internal bool ReceivingFile = false;
        internal bool SendingFile = false;
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
        private long received;
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
            FileTransferFinished?.Invoke(this, new EventArgs());
        }
        //  events>
        // <functions
        internal P08FileHeader GetHeaderPacket(string path)
        {
            try
            {
                FileInfo fi = new FileInfo(path);
                return new P08FileHeader(fi.Name, Convert.ToUInt64(fi.Length), fi.Attributes, fi.CreationTime, fi.LastAccessTime, fi.LastWriteTime, new byte[0], new byte[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        internal void SetHeaderPacket(string path, P08FileHeader packet)
        {
            string newPath = "";
            try
            {
                newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), packet.Name);
                File.Move(path, newPath);
                FileInfo fi = new FileInfo(newPath);
                fi.Attributes = packet.Attributes;
                fi.CreationTime = packet.CreationTime;
                fi.LastAccessTime = packet.LastAccessTime;
                fi.LastWriteTime = packet.LastWriteTime;
            }
            catch (IOException ex)
            {
                Console.WriteLine(string.Format("Source={0}, Target={1}, Exception{2}", path, newPath, ex));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
            await t;
        }
        internal async void OnDataBlockReceived(P09FileDataBlock packet)
        {
            if (stream == null)
                Console.WriteLine("Waiting for stream to be initialized");
            while (stream == null)
            {
                await Task.Delay(100);
            }
            Console.WriteLine("Stream initialized");
            Task r = parent.manager.SendPacketAsync(new P06Accepted(true, 9, ProblemCategory.None));
            Task w = stream.WriteAsync(packet.DataBlock, 0, packet.DataBlock.Length);
            await r;
            await w;
            received += packet.DataBlock.Length;
            if (received == length)
            {
                stream.Close();
                SetHeaderPacket(Path, header);
                ReceivingFile = false;
                OnFileTransferFinished();
                Reset();
            }
        }
        #endregion
        internal async void SendFile()
        {
            SendingFile = true;
            header = GetHeaderPacket(Path);
            Task t = parent.manager.SendPacketAsync(header);
            try
            {
                stream = new FileStream(Path, FileMode.Open);
            }
            catch (Exception ex)
            {
                parent.ExceptionHandler.PrintException(ex);
            }
            await t;
        }
        internal virtual async void OnAccepted(P06Accepted p)
        {
            if (SendingFile)
            {
                byte[] buf = new byte[131072];
                ulong startPos = Convert.ToUInt64(stream.Position);
                int count = await stream.ReadAsync(buf, 0, 131072);
                if (count == 0)
                {
                    stream.Close();
                    OnFileTransferFinished();
                    Reset();
                }
                else
                    await parent.manager.SendPacketAsync(new P09FileDataBlock(startPos, buf.Take(count).ToArray()));
            }
        }
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
            received = 0;
            length = 0;
            header = null;
        }
        //  functions>
    }
}