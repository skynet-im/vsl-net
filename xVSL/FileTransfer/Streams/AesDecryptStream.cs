using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VSL.FileTransfer.Streams
{
    internal class AesDecryptStream : Stream
    {
        private Stream stream;
        private ICryptoTransform aes;
        private CryptoStreamMode mode;
        private byte[] m_buffer;
        private bool m_finished = false;

        internal AesDecryptStream(Stream stream, ICryptoTransform aes, CryptoStreamMode mode)
        {
            this.stream = stream;
            this.aes = aes;
            this.mode = mode;
        }

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

        /// <summary>
        /// Decrypts the last block and writes plaintext data on the underlying stream. This operation can only performed once!
        /// </summary>
        public override void Flush()
        {
            if (mode != CryptoStreamMode.Write) throw new InvalidOperationException("You cannot flush a stream in read mode.");
            byte[] buf = aes.TransformFinalBlock(m_buffer, 0, m_buffer.Length);
            m_buffer = null;
            stream.Write(buf, 0, buf.Length);
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (mode != CryptoStreamMode.Read) throw new InvalidOperationException("You cannot read from a stream in write mode.");
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
            byte[] buf = new byte[newData + 16];
            int tmp_length = ReadDecrypt(buf, 0, newData);
            if (tmp_length < count - done) // first block will not be decrypted on first attempt
                tmp_length += ReadDecrypt(buf, tmp_length, 16);
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

        /// <summary>
        /// Decrypts full blocks and returns the count of decrypted bytes. This count can be up to 16 bytes less or more than expected.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private int ReadDecrypt(byte[] buffer, int offset, int count)
        {
            if (count % 16 != 0)
                throw new ArgumentException("You can only decrypt full blocks", "count");
            if (m_finished)
                return 0;
            byte[] buf = new byte[count];
            int read = stream.Read(buf, 0, count);
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (mode != CryptoStreamMode.Write) throw new InvalidOperationException("You cannot write on a stream in read mode.");
            if (offset + count < buffer.Length) throw new ArgumentOutOfRangeException("Buffer to small for combination of this offset and count.");
            int buflen = m_buffer != null ? m_buffer.Length : 0;
            if (buflen + count >= 32)
            {
                int decryptLength = buflen + count - 16;
                decryptLength = decryptLength - decryptLength % 16;
                byte[] buf = new byte[buflen + count];
                if (buflen > 0)
                    Array.Copy(m_buffer, buf, buflen);
                Array.Copy(buffer, offset, buf, buflen, count);
                WriteDecrypt(buf, 0, decryptLength);
                byte[] _buffer = new byte[buf.Length - decryptLength];
                Array.Copy(buf, decryptLength, _buffer, 0, _buffer.Length);
                m_buffer = _buffer;
            }
            else
            {
                byte[] _buffer = new byte[buflen + count];
                if (buflen > 0)
                    Array.Copy(m_buffer, _buffer, buflen);
                m_buffer = _buffer;
            }
        }

        private void WriteDecrypt(byte[] buffer, int offset, int count)
        {
            if (count % 16 != 0)
                throw new ArgumentException("You can only decrypt full blocks", "count");
            byte[] buf = new byte[count];
            int len = aes.TransformBlock(buffer, offset, count, buf, 0);
            stream.Write(buf, 0, len);
        }
    }
}
