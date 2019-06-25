using Nethereum.Hex.HexConvertors.Extensions;
using Plasma.RootChain.Contracts;
using PlasmaCore.EIP712;
using System;

namespace PlasmaCore.Transactions
{
    /// <summary>
    /// Factory for obtaining transaction encoders based on Plasma version
    /// </summary>
    public static class TransactionEncoderFactory
    {
        /// <summary>
        /// Creates transaction encoder based on Plasma version
        /// </summary>
        /// <param name="rootChainVersion">version of plasma. Defaults to RootChainABI.DefaultVersion</param>
        /// <param name="rootChainAddress">root chain address</param>
        /// <returns></returns>
        public static ITransactionEncoder Create(RootChainVersion rootChainVersion, string rootChainAddress)
        {
            if(rootChainVersion == RootChainVersion.Default)
            {
                rootChainVersion = RootChainABI.DefaultVersion;
            }

            if (rootChainVersion == RootChainVersion.Ari)
            {
                return new RawTransactionEncoder();
            }
            else if (rootChainVersion == RootChainVersion.Samrong)
            {
                EIP712Domain defaultDomain = new EIP712Domain(
                    "OMG Network",
                    "1",
                    rootChainAddress,
                    "0xfad5c7f626d80f9256ef01929f3beb96e058b8b4b0e3fe52d84f054c0e2a7a83".HexToByteArray());

                return new TypedDataTransactionEncoder(defaultDomain);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
