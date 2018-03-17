using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace VSL.FileTransfer.Streams
{
    [SecuritySafeCritical]
    internal class ShaStream : HashStream
    {
        private CryptoStream shaStream;
        private SHA256CryptoServiceProvider sha;

        internal ShaStream(Stream stream, CryptoStreamMode mode) : base(stream, mode)
        {
            sha = new SHA256CryptoServiceProvider();
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
