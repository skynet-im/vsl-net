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
    /// The server implementation of VSL file transfer.
    /// </summary>
    public class FileTransferServer : FileTransferSocket
    {
        // <fields
        new internal VSLServer parent;
        //  fields>
        // <constructor
        internal FileTransferServer(VSLServer parent)
        {
            this.parent = parent;
            base.parent = parent;
            InitializeComponent();
        }
        //  constructor>
        // <events
        /// <summary>
        /// The FileTransferRequested event occurs when the client has requested a VSL file transfer.
        /// </summary>
        public event EventHandler FileTransferRequested;
        /// <summary>
        /// Raises the OnFileTransferRequested event.
        /// </summary>
        internal void OnFileTransferRequested(Identifier id, StreamMode mode)
        {
            ID = id;
            Mode = mode;
            parent.EventThread.QueueWorkItem((ct) => FileTransferRequested?.Invoke(this, new EventArgs()));
        }
        //  events>
        // <functions
        /// <summary>
        /// Accepts a pending VSL file transfer request.
        /// </summary>
        public async void Accept()
        {
            if (string.IsNullOrEmpty(Path))
                throw new ArgumentNullException("The specified Path must not be null");
            else if (Mode != StreamMode.UploadFile && !File.Exists(Path))
                throw new FileNotFoundException("The file for the VSL file transfer could not be found @" + Path);
            else
            {
                await Task.Run(() => parent.manager.SendPacket(new P06Accepted(true, 7, ProblemCategory.None)));
                switch (Mode)
                {
                    case StreamMode.GetHeader:
                        await Task.Run(() => parent.manager.SendPacket(GetHeaderPacket(Path)));
                        break;
                    case StreamMode.GetFile:
                        SendFile();
                        break;
                    case StreamMode.UploadFile:
                        ReceiveFile();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid StreamMode");
                }
            }
        }
        /// <summary>
        /// Denies a pending VSL file transfer request.
        /// </summary>
        public void Deny()
        {
            parent.manager.SendPacket(new P06Accepted(false, 7, ProblemCategory.None));
            Reset();
        }
        internal override void Reset()
        {
            base.Reset();
        }
        //  functions>
    }
}