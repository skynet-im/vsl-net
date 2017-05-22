using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSL
{
    internal static class VersionManager
    {
        internal static bool IsVSLVersionSupported(ushort latestVSL, ushort oldestVSL)
        {
            return latestVSL >= Constants.CompatibilityVersion || Constants.VersionNumber >= oldestVSL;
        }
        internal static bool IsProductVersionSupported(ushort latestProductServer, ushort oldestProductServer, ushort latestProduct, ushort oldestProduct)
        {
            return latestProduct >= oldestProductServer || latestProductServer >= oldestProduct;
        }
    }
}