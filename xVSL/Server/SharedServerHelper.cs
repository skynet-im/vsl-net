using System;
using System.Collections.Generic;
using System.Text;

namespace VSL.Server
{
    /// <summary>
    /// Provides functions for performance optimizing that are shared between multiple VSL instances.
    /// </summary>
    public class SharedServerHelper
    {
        public ushort LatestProduct { get; }
        public ushort OldestProduct { get; }
        public string Keypair { get; }
        public ThreadMgr.InvokeMode InvokeMode { get; }
        public SharedServerHelper(ushort latestProduct, ushort oldestProduct, string keypair, ThreadMgr.InvokeMode invokeMode)
        {
            LatestProduct = latestProduct;
            OldestProduct = oldestProduct;
            Keypair = keypair;
            InvokeMode = invokeMode;
        }
    }
}