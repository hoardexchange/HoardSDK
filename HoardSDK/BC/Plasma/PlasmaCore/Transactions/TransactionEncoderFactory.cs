using Plasma.RootChain.Contracts;
using System;

namespace PlasmaCore.Transactions
{
    public static class TransactionEncoderFactory
    {
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
