using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VSL.FileTransfer.Streams
{
    public class AesEncryptStream : Stream
    {
        private Stream stream;
        private ICryptoTransform aes;
        private CryptoStreamMode mode;

        public override bool CanRead => mode == CryptoStreamMode.Read;
        public override bool CanSeek => false;
        public override bool CanWrite => mode == CryptoStreamMode.Write;
        /// <summary>
        /// This function is not implemented.
        /// </summary>
        public override long Length => throw new NotImplementedException();
        /// <summary>
        /// This function is not implemented.
        /// </summary>
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        // TODO: Implement Flush for CryptoStreamMode.Write
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement Read
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function is not implemented.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function is not implemented. The length will automatically be copied from the source stream.
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        // TODO: Implement Write
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
