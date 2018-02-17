using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VSL.FileTransfer.Streams
{
    /// <summary>
    /// A base class for <see cref="CryptoStream"/> implementations that compute a hash over the complete stream.
    /// </summary>
    internal abstract class HashStream : Stream
    {
        protected Stream stream;
        protected CryptoStreamMode mode;
        protected long _position = 0;

        public HashStream(Stream stream, CryptoStreamMode mode)
        {
            this.stream = stream ?? throw new ArgumentNullException("stream");
            if (mode == CryptoStreamMode.Read && !stream.CanRead)
                throw new ArgumentException("You cannot use CryptoStreamMode.Read with a not readable stream.");
            else if (mode == CryptoStreamMode.Write && !stream.CanWrite)
                throw new ArgumentException("You cannot use CryptoStreamMode.Write with a not writeable stream.");
        }

        public override bool CanRead => mode == CryptoStreamMode.Read;
        public override bool CanSeek => false;
        public override bool CanWrite => mode == CryptoStreamMode.Write;

        public byte[] Hash { get; protected set; }
        public override long Length => throw new NotSupportedException();
        /// <summary>
        /// Gets the current position in the top layer stream. The position in underlying streams are ignored.
        /// </summary>
        public override long Position { get => _position; set => throw new NotSupportedException(); }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
