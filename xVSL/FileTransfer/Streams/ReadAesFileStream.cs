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
        private byte[] m_buffer;
        private bool m_finished = false;

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

            // Read from source and decrypt full blocks
            int newData = Crypt.Util.GetTotalSize(count - done, 16);
            byte[] buf = new byte[newData];
            int tmp_length = DecryptSafe(buf, 0, newData);

            if (tmp_length > count - done)
            {
                Array.Copy(buf, 0, buffer, offset + done, count - done); // copy result
                int excessCount = tmp_length - count + done;
                int oldLength = m_buffer != null ? m_buffer.Length : 0;
                byte[] new_m_buffer = new byte[excessCount + oldLength];
                if (oldLength > 0)
                    Array.Copy(m_buffer, new_m_buffer, oldLength);
                Array.Copy(buf, count - done, new_m_buffer, oldLength, excessCount);
                m_buffer = new_m_buffer;
                return count;
            }
            else
            {
                Array.Copy(buf, 0, buffer, offset + done, tmp_length); // copy result
                return tmp_length;
            }
        }

        private int DecryptSafe(byte[] buffer, int offset, int count)
        {
            if (count % 16 != 0)
                throw new ArgumentException("You can only decrypt full blocks", "count");
            if (m_finished)
                return 0;
            byte[] buf = new byte[count];
            int read = source.Read(buf, 0, count);
            if (read < count)
            {
                m_finished = true;
                int first = read - (read % 16);
                int done = aes.TransformBlock(buf, 0, first, buffer, 0);
                byte[] last = aes.TransformFinalBlock(buf, first, read % 16);
                Array.Copy(last, 0, buffer, offset + done, last.Length);
                done += last.Length;
                return done;
            }
            else
                return aes.TransformBlock(buf, 0, count, buffer, offset);
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
