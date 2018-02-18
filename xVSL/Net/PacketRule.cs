using System;
using System.Collections.Generic;
using System.Text;
using VSL.Packet;

namespace VSL.Net
{
    internal class PacketRule
    {
        internal IPacket Packet { get; }
        internal CryptoAlgorithm[] Algorithms { get; }

        internal PacketRule(IPacket packet, params CryptoAlgorithm[] algorithms)
        {
            Packet = packet;
            Algorithms = algorithms;
        }
    }
}