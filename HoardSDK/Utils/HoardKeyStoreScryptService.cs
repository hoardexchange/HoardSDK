using Nethereum.KeyStore;
using Nethereum.KeyStore.Model;

namespace Hoard.Utils
{
    /// <summary>
    /// Hoard key store scrypt service
    /// </summary>
    public class HoardKeyStoreScryptService : KeyStoreScryptService
    {
        /// <summary>
        /// Default scrypt params
        /// </summary>
        protected override ScryptParams GetDefaultParams()
        {
            return new ScryptParams { Dklen = 32, N = 262144, R = 1, P = 8 };
        }
    }
}
