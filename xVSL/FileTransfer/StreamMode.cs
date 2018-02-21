using System;

namespace VSL.FileTransfer
{
    /// <summary>
    /// Specifies a stream mode for VSL file transfer.
    /// </summary>
    public struct StreamMode
    {
        /// <summary>
        /// Downloads only the file header with important meta data.
        /// </summary>
        public static readonly StreamMode GetHeader = new StreamMode(0);
        /// <summary>
        /// Downloads the complete file.
        /// </summary>
        public static readonly StreamMode GetFile = new StreamMode(1);
        /// <summary>
        /// Uploads only the file header with important meta data.
        /// </summary>
        public static readonly StreamMode PushHeader = new StreamMode(2);
        /// <summary>
        /// Uploads the complete file.
        /// </summary>
        public static readonly StreamMode PushFile = new StreamMode(3);

        private readonly byte value;
        private StreamMode(byte value)
        {
            if (value > 3)
                throw new ArgumentOutOfRangeException("value");
            this.value = value;
        }

        /// <summary>
        /// Returns the inverted <see cref="StreamMode"/> (e.g. GetFile -> PushFile, etc.)
        /// </summary>
        /// <returns></returns>
        public StreamMode Inverse()
        {
            return new StreamMode(Convert.ToByte((value + 2) % 4));
        }

        /// <summary>
        /// Returns the binary expression of the inverted <see cref="StreamMode"/>
        /// </summary>
        /// <returns></returns>
        public byte InverseToByte()
        {
            return Convert.ToByte((value + 2) % 4);
        }

        /// <summary>
        /// Gets the inverted <see cref="StreamMode"/> from a binary expression.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static StreamMode InverseFromByte(byte value)
        {
            return new StreamMode(Convert.ToByte((value + 2) % 4));
        }

        /// <summary>
        /// Determines whether to <see cref="StreamMode"/>s are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is StreamMode))
            {
                return false;
            }

            var mode = (StreamMode)obj;
            return value == mode.value;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hashCode = 1113510858;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + value.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Returns the partial type name of this <see cref="StreamMode"/> (e.g. "StreamMode.GetHeader").
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (value)
            {
                case 0:
                    return "StreamMode.GetHeader";
                case 1:
                    return "StreamMode.GetFile";
                case 2:
                    return "StreamMode.PushHeader";
                case 3:
                    return "StreamMode.PushFile";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Determines whether two <see cref="StreamMode"/>s are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(StreamMode left, StreamMode right)
        {
            return left.value == right.value;
        }

        /// <summary>
        /// Determines whether two <see cref="StreamMode"/>s are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(StreamMode left, StreamMode right)
        {
            return left.value != right.value;
        }

        /// <summary>
        /// Converts a <see cref="StreamMode"/> to a <see cref="byte"/>.
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator byte(StreamMode value)
        {
            return value.value;
        }

        /// <summary>
        /// Converts a <see cref="byte"/> to a <see cref="StreamMode"/>.
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator StreamMode(byte value)
        {
            return new StreamMode(value);
        }
    }
}