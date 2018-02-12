using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace System.Security.Cryptography
{
    /// <summary>
    /// Provides a wrapper around the Universal Windows Cryptographic API for the common .NET CAPI.
    /// </summary>
    public class SHA256CryptoServiceProvider : SHA256, ICryptoTransform
    {
        private HashAlgorithmProvider provider;
        private CryptographicHash csp;

        public SHA256CryptoServiceProvider()
        {
            provider = HashAlgorithmProvider.OpenAlgorithm("SHA256");
            csp = provider.CreateHash();
        }

        public bool CanReuseTransform => true;

        public bool CanTransformMultipleBlocks => true;

        public int InputBlockSize => 1;

        public int OutputBlockSize => 1;

        public override void Initialize()
        {

        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer == null)
                throw new ArgumentNullException("inputBuffer");

            HashCore(inputBuffer, inputOffset, inputCount);

            if (outputBuffer != null)
                Array.Copy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (inputBuffer == null)
                throw new ArgumentNullException("inputBuffer");

            HashCore(inputBuffer, inputOffset, inputCount);

            return HashFinal();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            byte[] buffer = new byte[cbSize];
            Array.Copy(array, ibStart, buffer, 0, cbSize);
            csp.Append(CryptographicBuffer.CreateFromByteArray(buffer));
        }

        protected override byte[] HashFinal()
        {
            IBuffer buffer = csp.GetValueAndReset();
            CryptographicBuffer.CopyToByteArray(buffer, out byte[] final);
            return final;
        }
    }
}
