using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VSL.Network;

namespace VSL
{
    internal static class VersionManager
    {
        private static Dictionary<ushort, string> versions;
        private static Dictionary<ushort, CryptoAlgorithm> netAlgs;
        static VersionManager()
        {
            versions = new Dictionary<ushort, string>()
            {
                { 0, "1.0" },
                { 1, "1.1" },
                { 2, "1.2" },
                { 3, "1.3" }
            };
            netAlgs = new Dictionary<ushort, CryptoAlgorithm>()
            {
                { 0, CryptoAlgorithm.AES_256_CBC_SP },
                { 1, CryptoAlgorithm.AES_256_CBC_SP },
                { 2, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3 },
                { 3, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_CTR }
            };
        }

        internal static string GetVersion(ushort? versionCode)
        {
            if (!versionCode.HasValue)
                return "waiting for version";
            else if (versions.TryGetValue(versionCode.Value, out string value))
                return value;
            else
                return "unknown version";
        }

        /// <summary>
        /// Gets the latest version that is supported by both parties. If no matching version could be found the value is null.
        /// </summary>
        internal static ushort? GetSharedVersion(ushort latestServer, ushort oldestServer, ushort latestClient, ushort oldestClient)
        {
            if (latestClient == latestServer) // same version
                return latestClient;
            if (latestClient > latestServer && oldestClient <= latestServer) // newer client
                return latestServer;
            if (latestClient < latestServer && latestClient >= oldestServer) // newer server
                return latestClient;
            return null;
        }

        /// <summary>
        /// Returns the default <see cref="CryptoAlgorithm"/> used for packet encryption.
        /// </summary>
        internal static CryptoAlgorithm GetNetworkAlgorithm(ushort? connectionVersion)
        {
            if (connectionVersion.HasValue && netAlgs.TryGetValue(connectionVersion.Value, out CryptoAlgorithm alg))
                return alg;
            else
                return CryptoAlgorithm.None;
        }
    }
}