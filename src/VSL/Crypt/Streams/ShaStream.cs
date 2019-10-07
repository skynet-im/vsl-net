using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace VSL.Crypt.Streams
{
    internal class ShaStream : HashStream
    {
        private readonly CryptoStream shaStream;
        private readonly SHA256 sha;

        internal ShaStream(Stream stream, CryptoStreamMode mode) : base(stream, mode)
        {
            sha = SHA256.Create();
            shaStream = new CryptoStream(stream, sha, mode);
        }

        public override byte[] Hash => sha?.Hash;

        public override void FlushFinalBlock()
        {
            if (HasFlushedFinalBlock)
                throw new InvalidOperationException("You cannot flush the final block twice.");
            shaStream.FlushFinalBlock();
            HasFlushedFinalBlock = true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (mode != CryptoStreamMode.Read)
                throw new InvalidOperationException("You cannot read from a stream in write mode.");
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            int done = shaStream.Read(buffer, offset, count);
            _position += done;
            return done;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (mode != CryptoStreamMode.Read)
                throw new InvalidOperationException("You cannot read from a stream in write mode.");
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            int done = await shaStream.ReadAsync(buffer, offset, count).ConfigureAwait(false);
            _position += done;
            return done;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (mode != CryptoStreamMode.Write)
                throw new InvalidOperationException("You cannot write on a stream in read mode.");
            if (HasFlushedFinalBlock)
                throw new InvalidOperationException("You cannot write on the stream when the final block was already flushed.");
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            shaStream.Write(buffer, offset, count);
            _position += count;
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (mode != CryptoStreamMode.Write)
                throw new InvalidOperationException("You cannot write on a stream in read mode.");
            if (HasFlushedFinalBlock)
                throw new InvalidOperationException("You cannot write on the stream when the final block was already flushed.");
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            await shaStream.WriteAsync(buffer, offset, count).ConfigureAwait(false);
            _position += count;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    shaStream.Dispose(); // all chained streams will be disposed with this call
                    sha.Dispose();
                }

                disposedValue = true;
            }
        }
        #endregion
    } // class
} // namespace
