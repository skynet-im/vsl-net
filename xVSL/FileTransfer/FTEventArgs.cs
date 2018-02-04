﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VSL.Crypt;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Represents a running or upcoming file transfer and provides all required information. Use with <see cref="FTSocket"/>. Design inspired by <see cref="System.Net.Sockets.SocketAsyncEventArgs"/>.
    /// </summary>
    public class FTEventArgs : EventArgs
    {
        // TODO: Assign this field at the first call.
        private FTSocket parent;
        internal FileStream FileStream;

        /// <summary>
        /// Initalizes a new instance of the <see cref="FTEventArgs"/> that can be used to send the associated file.
        /// </summary>
        /// <param name="identifier">A universal identifier to specify the file to process.</param>
        /// <param name="meta">Meta data of the file and required cryptographic keys.</param>
        /// <param name="path">The path where file currently exists or will be stored.</param>
        public FTEventArgs(Identifier identifier, FileMeta meta, string path)
        {
            Identifier = identifier;
            FileMeta = meta;
            Path = path;
        }

        internal FTEventArgs(Identifier identifier, StreamMode mode)
        {
            Identifier = identifier;
            Mode = mode;
        }

        public event EventHandler Canceled;
        public event EventHandler Finished;

        public Identifier Identifier { get; }
        public StreamMode Mode { get; internal set; }
        public FileMeta FileMeta { get; }
        public string Path { get; }
        public ContentAlgorithm HeaderAlgorithm { get; private set; }
        public ContentAlgorithm FileAlgorithm { get; private set; }

        internal void OnHeaderReceived()
        {

        }
    }
}