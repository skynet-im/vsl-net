using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSL
{
    /// <summary>
    /// Responsible for cryptography management
    /// </summary>
    internal abstract class NetworkManager
    {
        // <fields
        internal VSLSocket parent;
        //  fields>
        // <constructor
        //  constructor>
        // <functions
        internal async Task OnDataReceive()
        {
            try
            {
                CryptographicAlgorithm algorithm;
                try
                {
                    algorithm = (CryptographicAlgorithm)(await parent.channel.ReadAsync(1))[0];
                }
                catch (InvalidCastException ex)
                {
                    Console.WriteLine("[VSL] Cryptographic algorithm not supported: " + ex.ToString());
                    return;
                }
                switch (algorithm)
                {
                    case CryptographicAlgorithm.None:
                        break;
                    case CryptographicAlgorithm.RSA_2048:
                        break;
                    case CryptographicAlgorithm.AES_256:
                        break;
                }
            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.HandleReceiveTimeoutException(ex);
            }
        }
        internal async Task ReceivePacket_Plaintext()
        {
            try
            {

            }
            catch (TimeoutException ex)
            {
                parent.ExceptionHandler.HandleReceiveTimeoutException(ex);
            }
        }
        internal abstract Task ReceivePacket_RSA_2048();
        internal abstract Task ReceivePacket_AES_256();
        //  functions>
    }
}