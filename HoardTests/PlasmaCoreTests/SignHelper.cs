using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using PlasmaCore.Transactions;

namespace HoardTests.PlasmaCoreTests
{
    public static class PlasmaCoreTestsHelper
    {
        public static string Sign(ITransactionEncoder txEncoder, Transaction transaction, string address, string privateKey)
        {
            byte[] encodedTx = txEncoder.EncodeRaw(transaction);
            var rawHash = new Sha3Keccack().CalculateHash(encodedTx);
            var ecKey = new Nethereum.Signer.EthECKey(privateKey);
            var ecdsaSignature = ecKey.SignAndCalculateV(rawHash);
            string signature = Nethereum.Signer.EthECDSASignature.CreateStringSignature(ecdsaSignature);
            transaction.SetSignature(address, signature.HexToByteArray());
            return txEncoder.EncodeSigned(transaction).ToHex(true);
        }
    }
}
