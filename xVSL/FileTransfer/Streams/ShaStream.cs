using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VSL.FileTransfer.Streams
{
    internal class ShaStream : HashStream
    {
        private CryptoStream shaStream;
        private SHA256CryptoServiceProvider sha;

        internal ShaStream(Stream stream, CryptoStreamMode mode) : base(stream, mode)
        {
            sha = new SHA256CryptoServiceProvider();
            shaStream = new CryptoStream(stream, sha, mode);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (mode != CryptoStreamMode.Read)
                throw new InvalidOperationException("You cannot read from a stream in write mode.");

            int done = shaStream.Read(buffer, offset, count);
            _position += done;
            return done;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (mode != CryptoStreamMode.Write)
                throw new InvalidOperationException("You cannot write on a stream in read mode.");

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
                }
                try
                {
                    Hash = sha.Hash;
                }
                catch { }

                disposedValue = true;
            }
        }
        #endregion
    } // class
} // namespace
