using Plasma.RootChain.Contracts;
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
        /// <returns></returns>
        public static ITransactionEncoder Create(RootChainVersion rootChainVersion)
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
                return new TypedDataTransactionEncoder();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
