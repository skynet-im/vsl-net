using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.ApplicationModel;
#endif

namespace VSL
{
    /// <summary>
    /// Provides general information about VSL.
    /// </summary>
    public static class Constants
    {
        static Constants()
        {
#if WINDOWS_UWP
            PackageVersion version = Package.Current.Id.Version;
            _productVersion = new Version(version.Major, version.Minor, version.Build, version.Revision);
#else
            _productVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
#endif
        }
        /// <summary>
        /// The installed version as ushort.
        /// </summary>
        public const ushort VersionNumber = 2;
        /// <summary>
        /// The oldest supported version of VSL.
        /// </summary>
        public const ushort CompatibilityVersion = 1;
        /// <summary>
        /// The default size of the receive buffer of the Socket.
        /// </summary>
        public const int ReceiveBufferSize = 65536;
        /// <summary>
        /// The count of milliseconds the socket waits to receive a complete packet.
        /// </summary>
        public const int ReceiveTimeout = 5000;
        /// <summary>
        /// The count of bytes that must be received within one second.
        /// </summary>
        public const int ReceiveBandwith = 8000;
        /// <summary>
        /// The maximum admissible packet size. If a received packet is bigger the receiver closes the connection.
        /// </summary>
        public const int MaxPacketSize = 1048576;
        /// <summary>
        /// The default sleep time for background threads while waiting for work.
        /// </summary>
        public const int SleepTime = 10;
        private static Version _productVersion;
        /// <summary>
        /// Returns the product version of the current assembly with the specified precision.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ProductVersion(int length)
        {
            switch (length)
            {
                case 1:
                    return _productVersion.Major.ToString();
                case 2:
                    return string.Join(".", _productVersion.Major, _productVersion.Minor);
                case 3:
                    return string.Join(".", _productVersion.Major, _productVersion.Minor, _productVersion.Build);
                case 4:
                    return string.Join(".", _productVersion.Major, _productVersion.Minor, _productVersion.Build, _productVersion.Revision);
                default:
                    return null;
            }
        }
    }
}