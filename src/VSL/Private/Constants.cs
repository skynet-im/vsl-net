using System;
using System.Reflection;

namespace VSL
{
    internal static class Constants
    {
        /// <summary>
        /// Get the current assembly version of VSL.
        /// </summary>
        public static readonly Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        
        /// <summary>
        /// The latest supported protocol version.
        /// </summary>
        public const ushort ProtocolVersion = 2;

        /// <summary>
        /// The oldest supported protocol version.
        /// </summary>
        public const ushort CompatibilityVersion = 1;

        /// <summary>
        /// The count of internal packets. This important to split the ID range in two spaces.
        /// </summary>
        public const int InternalPacketCount = 10;
    }
}