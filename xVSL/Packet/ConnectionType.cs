using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal enum ConnectionState : byte
    {
        CompatibilityMode,
        Redirect,
        NotCompatible,
        Compatible
    }
}