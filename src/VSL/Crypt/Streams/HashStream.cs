using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace VSL.Crypt.Streams
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
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (mode == CryptoStreamMode.Read && !stream.CanRead)
                throw new ArgumentException("You cannot use CryptoStreamMode.Read with a not readable stream.");
            else if (mode == CryptoStreamMode.Write && !stream.CanWrite)
                throw new ArgumentException("You cannot use CryptoStreamMode.Write with a not writeable stream.");
            this.mode = mode;
        }

        public sealed override bool CanRead => mode == CryptoStreamMode.Read;
        public sealed override bool CanSeek => false;
        public sealed override bool CanWrite => mode == CryptoStreamMode.Write;

        public bool HasFlushedFinalBlock { get; protected set; }
        /// <summary>
        /// Gets the hash of all data read or written on this stream.
        /// </summary>
        public abstract byte[] Hash { get; }
        /// <summary>
        /// This property is not supported because a <see cref="CryptoStream"/> has no constant length.
        /// </summary>
        public sealed override long Length => throw new NotSupportedException();
        /// <summary>
        /// Gets the current position in the top layer stream. The position in underlying streams are ignored.
        /// </summary>
        public sealed override long Position { get => _position; set => throw new NotSupportedException(); }

        /// <summary>
        /// This method does not do anything. If you have finished working on this stream call <see cref="FlushFinalBlock"/> and dispose the stream.
        /// </summary>
        public override void Flush() { }
        /// <summary>
        /// This method does not do anything. If you have finished working on this stream call <see cref="FlushFinalBlock"/> and dispose the stream.
        /// </summary>
        public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        /// <summary>
        /// Updates the underlying data source or repository with the current state of the buffer, then clears the buffer.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="CryptographicException"/>
        public abstract void FlushFinalBlock();
        /// <summary>
        /// This function is not supported because <see cref="CryptoStream"/>s are unseekable.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public sealed override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        /// <summary>
        /// This function is not supported because a <see cref="HashStream"/> only operates on an underlying stream.
        /// </summary>
        /// <param name="value"></param>
        public sealed override void SetLength(long value) => throw new NotSupportedException();
    }
}
