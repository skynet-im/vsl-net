using System;
using System.Collections.Generic;
using System.Text;
using VSL.Packet;

namespace VSL.Network
{
    internal struct PacketRule
    {
        internal bool Available { get; }
        internal IPacket Packet { get; }
        private readonly bool[] algorithms;

        internal PacketRule(IPacket packet, params CryptoAlgorithm[] algs)
        {
            Packet = packet;
            algorithms = new bool[4];
            foreach (CryptoAlgorithm alg in algs)
                algorithms[(byte)alg] = true;
            Available = true;
        }

        /// <summary>
        /// Verifies that a provided algorithm is supported by the packet.
        /// </summary>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        internal bool VerifyAlgorithm(CryptoAlgorithm algorithm)
        {
            return algorithms[(byte)algorithm];
        }
    }
}