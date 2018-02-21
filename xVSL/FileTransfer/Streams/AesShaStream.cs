using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VSL.Crypt;

namespace VSL.FileTransfer.Streams
{
    internal class AesShaStream : HashStream
    {
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
            this.key = key ?? throw new ArgumentNullException("key");
            if (key.Length != 32)
                throw new ArgumentOutOfRangeException("key", "The AES key must have a length of 256 bit.");
            if (operation == CryptographicOperation.Encrypt)
                iv = AesStatic.GenerateIV();
            else if (operation == CryptographicOperation.Decrypt) { }
            else
                throw new NotSupportedException("This stream does not support cryptographic operations other than encrypt and decrypt.");

            this.operation = operation;
            sha = new SHA256CryptoServiceProvider();
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
                    Array.Copy(iv, 0, buffer, offset, 16); // copy iv to encrypted output
                    done += 16;
                    offset += 16;
                    count -= 16;
#if WINDOWS_UWP
                    csp = Aes.Create();
#else
                    csp = new AesCryptoServiceProvider();
#endif
                    transform = csp.CreateEncryptor(key, iv);
                    shaStream = new CryptoStream(stream, sha, CryptoStreamMode.Read); // compute SHA256 of plain data
                    aesStream = new CryptoStream(shaStream, transform, CryptoStreamMode.Read); // encrypt after computing hash
                    first = false;
                }
                done += aesStream.Read(buffer, offset, count);
                _position += done; // The method returns the iv so it has to be counted
                return done;
            }
            else // CryptographicOperation.Decrypt
            {
                if (first)
                {
                    iv = new byte[16];
                    if (stream.Read(iv, 0, 16) < 16) return -1; // read iv from stream
#if WINDOWS_UWP
                    csp = Aes.Create();
#else
                    csp = new AesCryptoServiceProvider();
#endif
                    transform = csp.CreateDecryptor(key, iv);
                    aesStream = new CryptoStream(stream, transform, CryptoStreamMode.Read); // first decrypt data
                    shaStream = new CryptoStream(aesStream, sha, CryptoStreamMode.Read); // then compute hash of the plain data
                    first = false;
                }
                int done = shaStream.Read(buffer, offset, count);
                _position += done; // The method does not return an iv and its length is ignored.
                return done;
            }
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
            if (operation == CryptographicOperation.Encrypt)
            {
                if (first)
                {
                    stream.Write(iv, 0, 16); // write pre-generated iv on stream
#if WINDOWS_UWP
                    csp = Aes.Create();
#else
                    csp = new AesCryptoServiceProvider();
#endif
                    transform = csp.CreateEncryptor(key, iv);
                    aesStream = new CryptoStream(stream, transform, CryptoStreamMode.Write); // write encrypted data on stream
                    shaStream = new CryptoStream(aesStream, sha, CryptoStreamMode.Write); // compute hash before encrypting
                    first = false;
                }
                shaStream.Write(buffer, offset, count);
                _position += count; // no iv is provided directly -> ignore the count of the iv
            }
            else // CryptographicOperation.Decrypt
            {
                int done = 0;
                if (first)
                {
                    if (count < 16) throw new ArgumentOutOfRangeException("count", count, "The first block must be at least 16 bytes large.");
                    iv = new byte[16];
                    Array.Copy(buffer, offset, iv, 0, 16); // read iv from buffer
                    offset += 16;
                    count -= 16;
                    done += 16;
#if WINDOWS_UWP
                    csp = Aes.Create();
#else
                    csp = new AesCryptoServiceProvider();
#endif
                    transform = csp.CreateDecryptor(key, iv);
                    shaStream = new CryptoStream(stream, sha, CryptoStreamMode.Write); // compute hash of plain data and write on stream
                    aesStream = new CryptoStream(shaStream, transform, CryptoStreamMode.Write); // decrypt before computing hash
                    first = false;
                }
                aesStream.Write(buffer, offset, count);
                done += count;
                _position += done; // the iv is directly provided and will be counted
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (mode == CryptoStreamMode.Read)
                    {
                        if (operation == CryptographicOperation.Encrypt)
                            aesStream.Dispose(); // all chained streams will be disposed with this call
                        else
                            shaStream.Dispose();
                    }
                    else // CryptoStreamMode.Write
                    {
                        if (operation == CryptographicOperation.Encrypt)
                            shaStream.Dispose();
                        else
                            aesStream.Dispose();
                    }
                    try
                    {
                        Hash = sha.Hash;
                    }
                    catch { }
                }

                disposedValue = true;
            }
        }
        #endregion
    } // class
} // namespace
