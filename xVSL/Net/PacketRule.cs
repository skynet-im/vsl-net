using System;
using System.Collections.Generic;
using System.Text;
using VSL.Packet;

namespace VSL.Net
{
    internal struct PacketRule
    {
        internal bool Available { get; }
        internal IPacket Packet { get; }
        private bool[] Algorithms;

        internal PacketRule(IPacket packet, params CryptoAlgorithm[] algorithms)
        {
            Packet = packet;
            Algorithms = new bool[4];
            foreach (CryptoAlgorithm alg in algorithms)
                Algorithms[(byte)alg] = true;
            Available = true;
        }

        internal bool VerifyAlgorithm(CryptoAlgorithm algorithm)
        {
            return Algorithms[(byte)algorithm];
        }
    }
}