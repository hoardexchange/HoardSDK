using Nethereum.Signer;
using Nethereum.Util;

namespace HoardTests.PlasmaCoreTests
{
    public static class PlasmaCoreTestsHelper
    {
        public static string Sign(byte[] encodedTx, string privateKey)
        {
            var rawHash = new Sha3Keccack().CalculateHash(encodedTx);
            var ecKey = new EthECKey(privateKey);
            var ecdsaSignature = ecKey.SignAndCalculateV(rawHash);
            return EthECDSASignature.CreateStringSignature(ecdsaSignature);
        }
    }
}
