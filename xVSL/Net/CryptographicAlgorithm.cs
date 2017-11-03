using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal enum CryptographicAlgorithm : byte
    {
        None,
        RSA_2048_OAEP,
        Insecure_AES_256_CBC,
        AES_256_CBC_MP2
    }
}