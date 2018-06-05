﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSL.Net;

namespace VSL
{
    internal static class VersionManager
    {
        private static Dictionary<ushort?, string> versions;
        private static Dictionary<ushort, CryptoAlgorithm> netAlgs;
        static VersionManager()
        {
            versions = new Dictionary<ushort?, string>()
            {
                { 0, "1.0" },
                { 1, "1.1" },
                { 2, "1.2" }
            };
            netAlgs = new Dictionary<ushort, CryptoAlgorithm>()
            {
                { 0, CryptoAlgorithm.AES_256_CBC_SP },
                { 1, CryptoAlgorithm.AES_256_CBC_SP },
                { 2, CryptoAlgorithm.AES_256_CBC_HMAC_SHA256_MP3 }
            };
        }

        internal static string GetVersion(ushort? versionCode)
        {
            if (!versionCode.HasValue)
                return "waiting for version";
            else if (versions.TryGetValue(versionCode, out string value))
                return value;
            else
                return "unknown version";
        }

        /// <summary>
        /// Gets the latest VSL version that is supported by both parties. If no matching version could be found the value is null.
        /// </summary>
        /// <param name="latestVSL"></param>
        /// <param name="oldestVSL"></param>
        /// <returns></returns>
        internal static ushort? GetSharedVSLVersion(ushort latestVSL, ushort oldestVSL)
        {
            if (latestVSL == Constants.VersionNumber) // same version
                return latestVSL;
            if (latestVSL > Constants.VersionNumber && oldestVSL <= Constants.VersionNumber) // newer client
                return Constants.VersionNumber;
            if (latestVSL < Constants.VersionNumber && latestVSL >= Constants.CompatibilityVersion) // newer server
                return latestVSL;
            return null;
        }

        /// <summary>
        /// Gets the latest product version that is supported by both parties. If no matching version could be found the value is null.
        /// </summary>
        /// <param name="latestServer"></param>
        /// <param name="oldestServer"></param>
        /// <param name="latestClient"></param>
        /// <param name="oldestClient"></param>
        /// <returns></returns>
        internal static ushort? GetSharedProductVersion(ushort latestServer, ushort oldestServer, ushort latestClient, ushort oldestClient)
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
        /// <param name="connectionVersion"></param>
        /// <returns></returns>
        internal static CryptoAlgorithm GetNetworkAlgorithm(ushort? connectionVersion)
        {
            if (!connectionVersion.HasValue || !netAlgs.TryGetValue(connectionVersion.Value, out CryptoAlgorithm alg))
                return CryptoAlgorithm.None;
            else
                return alg;
        }
    }
}