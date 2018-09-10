using System;
using System.Collections.Generic;
using System.Text;

namespace VSL.FileTransfer.Streams
{
    internal enum CryptographicOperation
    {
        None,
        Hash,
        Encrypt,
        Decrypt
    }
}
