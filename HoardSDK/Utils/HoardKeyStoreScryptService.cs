using Nethereum.KeyStore;
using Nethereum.KeyStore.Model;

namespace Hoard.Utils
{
    class HoardKeyStoreScryptService : KeyStoreScryptService
    {
        protected override ScryptParams GetDefaultParams()
        {
            return new ScryptParams { Dklen = 32, N = 262144, R = 1, P = 8 };
        }
    }
}
