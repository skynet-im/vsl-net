using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    /// <summary>
    /// Provides different identifiers for file transfers.
    /// </summary>
    public class Identifier
    {
        /// <summary>
        /// Gets the selected identification mode.
        /// </summary>
        public IdentificationMode Mode { get; }
        /// <summary>
        /// Creates a new instance of the Identifier class.
        /// </summary>
        /// <param name="id">Unsigned integer as identifier.</param>
        public Identifier(uint id)
        {
            Mode = IdentificationMode.UInt32;
        }
        /// <summary>
        /// Creates a new instance of the Identifier class.
        /// </summary>
        /// <param name="id">Unsigned long as identifier.</param>
        public Identifier(ulong id)
        {
            Mode = IdentificationMode.UInt64;
        }
        /// <summary>
        /// Creates a new instance of the Identifier class.
        /// </summary>
        /// <param name="id">byte array with max. 65536 bytes length as identifier.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Identifier(byte[] id)
        {
            Mode = IdentificationMode.ByteArray;
        }
        /// <summary>
        /// Creates a new instance of the Identifier class.
        /// </summary>
        /// <param name="id">string as identifier.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Identifier(string id)
        {
            Mode = IdentificationMode.ByteArray;
        }
        /// <summary>
        /// Supported identifier types.
        /// </summary>
        public enum IdentificationMode : byte
        {
            /// <summary>
            /// Unsigned integer as identifier.
            /// </summary>
            UInt32,
            /// <summary>
            /// Unsigned long as identifier.
            /// </summary>
            UInt64,
            /// <summary>
            /// Byte array as identifier.
            /// </summary>
            ByteArray,
            /// <summary>
            /// String (UTF-8) as identifier.
            /// </summary>
            String
        }
    }
}