using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using VSL.Crypt;

namespace VSL.FileTransfer.Streams
{
    [SecuritySafeCritical]
    internal class AesShaStream : HashStream
    {
        private CryptoStream topStream;
        private CryptoStream aesStream;
        private CryptoStream shaStream;
        private byte[] key;
        private byte[] iv;
        private Aes csp;
        private ICryptoTransform transform;
        private SHA256CryptoServiceProvider sha;
        private CryptographicOperation operation;
        private bool first = true;

        internal AesShaStream(Stream stream, byte[] key, CryptoStreamMode mode, CryptographicOperation operation) : base(stream, mode)
        {
            this.key = key ?? throw new ArgumentNullException(nameof(key));
            if (key.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(key), key.Length, "The AES key must have a length of 256 bit.");
            if (operation == CryptographicOperation.Encrypt)
                iv = AesStatic.GenerateIV();
            else if (operation == CryptographicOperation.Decrypt) { }
            else
                throw new NotSupportedException("This stream does not support cryptographic operations other than encrypt and decrypt.");

            this.operation = operation;
            sha = new SHA256CryptoServiceProvider();
        }

        public override byte[] Hash => sha?.Hash;

        public override void FlushFinalBlock()
        {
            if (HasFlushedFinalBlock)
                throw new InvalidOperationException("You cannot flush the final block twice.");

            topStream.FlushFinalBlock();
            HasFlushedFinalBlock = true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (mode != CryptoStreamMode.Read)
                throw new InvalidOperationException("You cannot read from a stream in write mode.");
            if (HasFlushedFinalBlock)
                throw new InvalidOperationException("You cannot write on the stream when the final block was already flushed.");
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            int done = 0;

            if (operation == CryptographicOperation.Encrypt)
            {
                if (first)
                {
                    Array.Copy(iv, 0, buffer, offset, 16); // copy iv to encrypted output
                    done += 16; // the method returns the iv so it has to be counted
                    offset += 16;
                    count -= 16;
                    csp = new AesCryptoServiceProvider();
                    transform = csp.CreateEncryptor(key, iv);
                    shaStream = new CryptoStream(stream, sha, CryptoStreamMode.Read); // compute SHA256 of plain data
                    aesStream = new CryptoStream(shaStream, transform, CryptoStreamMode.Read); // encrypt after computing hash
                    topStream = aesStream; // Following operations will be done on this stream.
                    first = false;
                }
            }
            else // CryptographicOperation.Decrypt
            {
                if (first)
                {
                    iv = new byte[16];
                    if (stream.Read(iv, 0, 16) < 16) return -1; // read iv from stream
                    // The method does not return an iv and its length is ignored.
                    csp = new AesCryptoServiceProvider();
                    transform = csp.CreateDecryptor(key, iv);
                    aesStream = new CryptoStream(stream, transform, CryptoStreamMode.Read); // first decrypt data
                    shaStream = new CryptoStream(aesStream, sha, CryptoStreamMode.Read); // then compute hash of the plain data
                    topStream = shaStream; // Following operations will be done on this stream.
                    first = false;
                }
            }
            done += topStream.Read(buffer, offset, count);
            _position += done;
            return done;
        }

        /// <summary>
        /// Processes the input data and writes the result on the underlying stream. The first write blocks must be at least 16 bytes large.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (mode != CryptoStreamMode.Write)
                throw new InvalidOperationException("You cannot write on a stream in read mode.");
            if (HasFlushedFinalBlock)
                throw new InvalidOperationException("You cannot write on the stream when the final block was already flushed.");
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            int done = 0;

            if (operation == CryptographicOperation.Encrypt)
            {
                if (first)
                {
                    stream.Write(iv, 0, 16); // write pre-generated iv on stream
                    // no iv is provided directly -> ignore the count of the iv
                    csp = new AesCryptoServiceProvider();
                    transform = csp.CreateEncryptor(key, iv);
                    aesStream = new CryptoStream(stream, transform, CryptoStreamMode.Write); // write encrypted data on stream
                    shaStream = new CryptoStream(aesStream, sha, CryptoStreamMode.Write); // compute hash before encrypting
                    topStream = shaStream; // Following operations will be done on this stream.
                    first = false;
                }
            }
            else // CryptographicOperation.Decrypt
            {
                if (first)
                {
                    if (count < 16) throw new ArgumentOutOfRangeException("count", count, "The first block must be at least 16 bytes large.");
                    iv = new byte[16];
                    Array.Copy(buffer, offset, iv, 0, 16); // read iv from buffer
                    offset += 16;
                    count -= 16;
                    done += 16; // the iv is directly provided and will be counted
                    csp = new AesCryptoServiceProvider();
                    transform = csp.CreateDecryptor(key, iv);
                    shaStream = new CryptoStream(stream, sha, CryptoStreamMode.Write); // compute hash of plain data and write on stream
                    aesStream = new CryptoStream(shaStream, transform, CryptoStreamMode.Write); // decrypt before computing hash
                    topStream = aesStream; // Following operations will be done on this stream.
                    first = false;
                }
            }
            topStream.Write(buffer, offset, count);
            _position += done + count;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    topStream.Dispose(); // This call will flush and dispose all chained streams.
                    transform.Dispose();
                    sha.Dispose();
                }

                disposedValue = true;
            }
        }
        #endregion
    } // class
} // namespace
