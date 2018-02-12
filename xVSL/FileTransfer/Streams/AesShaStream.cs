using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VSL.Crypt;

namespace VSL.FileTransfer.Streams
{
    internal class AesShaStream : Stream
    {
        private Stream stream;
        private CryptoStream aesStream;
        private CryptoStream shaStream;
        private byte[] key;
        private byte[] iv;
        private Aes csp;
        private ICryptoTransform transform;
        private SHA256CryptoServiceProvider sha;
        private CryptoStreamMode mode;
        private CryptographicOperation operation;
        private bool first = true;

        internal AesShaStream(Stream stream, byte[] key, CryptoStreamMode mode, CryptographicOperation operation)
        {
            this.stream = stream ?? throw new ArgumentNullException("stream");
            this.key = key ?? throw new ArgumentNullException("key");
            if (key.Length != 32)
                throw new ArgumentOutOfRangeException("key", "The AES key must have a length of 256 bit.");
            if (mode == CryptoStreamMode.Read && !stream.CanRead)
                throw new ArgumentException("You cannot use CryptoStreamMode.Read with a not readable stream.");
            else if (mode == CryptoStreamMode.Write && !stream.CanWrite)
                throw new ArgumentException("You cannot use CryptoStreamMode.Write with a not writeable stream.");
            if (operation == CryptographicOperation.Encrypt)
                iv = AesStatic.GenerateIV();
            else if (operation == CryptographicOperation.Decrypt) { }
            else
                throw new NotSupportedException("This stream does not support cryptographic operations other than encrypt and decrypt.");

            this.mode = mode;
            this.operation = operation;
            sha = new SHA256CryptoServiceProvider();
        }

        public override bool CanRead => mode == CryptoStreamMode.Read;
        public override bool CanSeek => false;
        public override bool CanWrite => mode == CryptoStreamMode.Write;
        public override long Length => throw new NotImplementedException();
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public byte[] Hash { get; private set; }

        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (mode != CryptoStreamMode.Read)
                throw new InvalidOperationException("You cannot read from a stream in write mode.");
            if (operation == CryptographicOperation.Encrypt)
            {
                int done = 0;
                if (first)
                {
                    Array.Copy(iv, 0, buffer, offset, 16);
                    done += 16;
                    offset += 16;
                    count -= 16;
#if WINDOWS_UWP
                    csp = Aes.Create();
#else
                    csp = new AesCryptoServiceProvider();
#endif
                    transform = csp.CreateEncryptor(key, iv);
                    shaStream = new CryptoStream(stream, sha, CryptoStreamMode.Read);
                    aesStream = new CryptoStream(shaStream, transform, CryptoStreamMode.Read);
                    first = false;
                }
                done += aesStream.Read(buffer, offset, count);
                return done;
            }
            else // CryptographicOperation.Decrypt
            {
                if (first)
                {
                    iv = new byte[16];
                    if (stream.Read(iv, 0, 16) < 16) return -1;
#if WINDOWS_UWP
                    csp = Aes.Create();
#else
                    csp = new AesCryptoServiceProvider();
#endif
                    transform = csp.CreateDecryptor(key, iv);
                    aesStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
                    shaStream = new CryptoStream(aesStream, sha, CryptoStreamMode.Read);
                    first = false;
                }
                return shaStream.Read(buffer, offset, count);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotImplementedException();

        public override void SetLength(long value)
            => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (mode != CryptoStreamMode.Write)
                throw new InvalidOperationException("You cannot write on a stream in read mode.");
            if (operation == CryptographicOperation.Encrypt)
            {
                if (first)
                {
                    // TODO: Generate iv and write on stream
                    // TODO: Initialize csp
                }
                // Encrypt and write
            }
            else
            {
                if (first)
                {
                    // TODO: Read iv from buffer
                    // TODO: Initialize csp
                }
                // Decrypt and write
            }
        }
    }
}
