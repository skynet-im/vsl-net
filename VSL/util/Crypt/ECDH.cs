using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VSL.Crypt
{
    /// <summary>
    /// The simple implementation of ECDH in VSL
    /// </summary>
    public static class ECDH
    {
        // © 2017 Daniel Lerch
        /// <summary>
        /// Generates an ECDH keypair that can be used for key-derivation-operations.
        /// </summary>
        /// <param name="privateKey">The private key in EccPrivateBlob-format.</param>
        /// <param name="publicKey">The public key in EccPublicBlob-format.</param>
        public static void GenerateKey(out byte[] privateKey, out byte[] publicKey)
        {
            CngKeyCreationParameters param = new CngKeyCreationParameters();
            param.ExportPolicy = CngExportPolicies.AllowPlaintextExport;
            using (CngKey key = CngKey.Create(CngAlgorithm.ECDiffieHellmanP256, null, param))
            {
                using (ECDiffieHellmanCng ecdh = new ECDiffieHellmanCng(key))
                {
                    ecdh.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                    ecdh.HashAlgorithm = CngAlgorithm.Sha256;
                    privateKey = ecdh.Key.Export(CngKeyBlobFormat.EccPrivateBlob);
                    publicKey = ecdh.PublicKey.ToByteArray();
                }
            }
        }

        /// <summary>
        /// Derives
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static byte[] DeriveKey(byte[] privateKey, byte[] publicKey)
        {
            byte[] final;
            using (CngKey key = CngKey.Import(privateKey, CngKeyBlobFormat.EccPrivateBlob))
            {
                using (ECDiffieHellmanCng ecdh = new ECDiffieHellmanCng(key))
                {
                    ecdh.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                    ecdh.HashAlgorithm = CngAlgorithm.Sha256;
                    final = ecdh.DeriveKeyMaterial(ECDiffieHellmanCngPublicKey.FromByteArray(publicKey, CngKeyBlobFormat.EccPublicBlob));
                }
            }
            return final;
        }
    }
}