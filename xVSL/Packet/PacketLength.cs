using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL.Packet
{
    internal abstract class PacketLength
    {
        internal abstract LengthType Type { get; }
        internal enum LengthType : byte
        {
            Constant,
            UInt32
        }
        internal abstract uint Length { get; }
    }
    internal class ConstantLength : PacketLength
    {
        internal ConstantLength(uint length)
        {
            Length = length;
        }
        internal override LengthType Type
        {
            get
            {
                return LengthType.Constant;
            }
        }
        internal override uint Length { get; }
    }
    internal class VariableLength : PacketLength
    {
        internal VariableLength()
        {

        }
        internal override LengthType Type
        {
            get
            {
                return LengthType.UInt32;
            }
        }
        /// <summary>
        /// This property is not implemented
        /// </summary>
        internal override uint Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}