using System;
using System.Collections.Generic;
using System.Text;
using VSL.Packet;

namespace VSL.Net
{
    internal class PacketRule
    {
        internal readonly IPacket Packet;
        internal readonly CryptoAlgorithm[] Algorithms;

        internal PacketRule(IPacket packet, params CryptoAlgorithm[] algorithms)
        {
            Packet = packet;
            Algorithms = algorithms;
        }
    }
}