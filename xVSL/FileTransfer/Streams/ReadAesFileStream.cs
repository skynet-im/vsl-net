using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace VSL.FileTransfer.Streams
{
    internal class ReadAesFileStream : Stream
    {
        private Stream source;
        private ICryptoTransform aes;
        private long m_sourceIdx;
        private byte[] m_buffer;

        internal ReadAesFileStream(Stream source, ICryptoTransform aes)
        {
            this.source = source;
            this.aes = aes;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        /// <summary>
        /// This function is not implemented.
        /// </summary>
        public override long Length => throw new NotImplementedException();
        /// <summary>
        /// This function is not implemented.
        /// </summary>
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// This function is not implemented.
        /// </summary>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Read from buffer
            int done = 0;
            if (m_buffer != null)
                if (m_buffer.Length > count)
                {
                    Array.Copy(m_buffer, 0, buffer, offset, count);
                    byte[] new_buffer = new byte[m_buffer.Length - count];
                    Array.Copy(m_buffer, count, new_buffer, 0, m_buffer.Length - count);
                    m_buffer = new_buffer;
                    return count;
                }
                else if (m_buffer.Length == count)
                {
                    Array.Copy(m_buffer, 0, buffer, offset, count);
                    m_buffer = null;
                    return count;
                }
                else
                {
                    Array.Copy(m_buffer, 0, buffer, offset, m_buffer.Length);
                    done = m_buffer.Length;
                    m_buffer = null;
                }

            // Read from source
            byte[] buf = new byte[count - done];
            int l_length = source.Read(buf, 0, count - done);
            bool isLastBlock = count - done > l_length;
            m_sourceIdx += l_length;

            // TODO: Decrypt blocks
            // TODO: Decrypt last block
            // TODO: Write buffer
            // TODO: Write in buffer
            // TODO: Return count
        }

        /// <summary>
        /// This function is not implemented.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function is not implemented. The length will automatically be copied from the source stream.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function is not implemented.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
