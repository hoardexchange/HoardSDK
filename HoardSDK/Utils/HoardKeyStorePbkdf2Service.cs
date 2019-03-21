using Nethereum.KeyStore;
using Nethereum.KeyStore.Model;

namespace Hoard.Utils
{
    class HoardKeyStorePbkdf2Service : KeyStorePbkdf2Service
    {
        protected override Pbkdf2Params GetDefaultParams()
        {
            return new Pbkdf2Params { Dklen = 32, Count = 262144, Prf = "hmac-sha256" };
        }
    }
}
