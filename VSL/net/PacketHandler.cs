using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Packet;

namespace VSL
{
    /// <summary>
    /// Handles internal VSL packets
    /// </summary>
    internal abstract class PacketHandler
    {
        // <fields
        internal List<IPacket> RegisteredPackets;
        internal VSLSocket parent;
        //  fields>

        internal PacketHandler()
        {
            InitializeComponent(); // TWOMETER-CORRECT: Vielleicht sollte man seine Packetliste auch initialisieren
        }

        // <constructor
        /// <summary>
        /// Initializes all non-child-specific components
        /// </summary>
        internal void InitializeComponent()
        {
            RegisteredPackets = new List<IPacket>
            {
                new P00Handshake(),
                new P01KeyExchange(),
                new P03FinishHandshake(),
                new P04ChangeIV()
            };
        }
        //  constructor>

        // <functions
        internal bool TryGetPacket(byte id, out IPacket packet)
        {
            foreach (IPacket p in RegisteredPackets)
            {
                if (id == p.ID)
                {
                    packet = p;
                    return true;
                }
            }

            packet = null;
            return false;
        }

        /// <summary>
        /// Handles an internal VSL packet
        /// </summary>
        /// <param name="id">Packet ID</param>
        /// <param name="content">Packet data</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        internal void HandleInternalPacket(byte id, byte[] content)
        {
            foreach (IPacket p in RegisteredPackets)
            {
                if (id == p.ID)
                {
                    IPacket packet = p.CreatePacket(content);
                    packet.HandlePacket(this);
                    return;
                }
            }
        }

        internal abstract void HandlePacket00Handshake(P00Handshake p);
        internal abstract void HandlePacket01KeyExchange(P01KeyExchange p);
        internal abstract void HandlePacket02Certificate(P02Certificate p);
        internal abstract void HandlePacket03FinishHandshake(P03FinishHandshake p);
        internal abstract void HandlePacket04ChangeIV(P04ChangeIV p);
        internal abstract void HandlePacket05KeepAlive(P05KeepAlive p);
        //  functions>
    }
}