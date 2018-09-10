using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal enum ConnectionState : byte
    {
        /// <summary>
        /// Connection with protocol version 1.1. This entry replaces <see cref="Compatible"/> at 0 from VSL 1.1.
        /// </summary>
        CompatibilityMode,
        /// <summary>
        /// The server cannot handle the requested version. The client is redirected to another server specified in <see cref="P03FinishHandshake"/>.
        /// </summary>
        Redirect,
        /// <summary>
        /// The connection was refused by the server because of incompatible versions.
        /// </summary>
        NotCompatible,
        /// <summary>
        /// Connection with protocol version 1.2 or higher. The specific version is specified in <see cref="P03FinishHandshake"/>.
        /// </summary>
        Compatible
    }
}