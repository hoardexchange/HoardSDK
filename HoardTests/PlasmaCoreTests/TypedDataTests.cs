using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using PlasmaCore.EIP712;
using PlasmaCore.Transactions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.PlasmaCoreTests
{
    public class TypedDataTests
    {
        private EIP712Domain omgDomain = new EIP712Domain(
            "OMG Network",
            "1",
            "0xecda6da8fddd416837bdcf38d6e17a4a898534eb",
            "0xfad5c7f626d80f9256ef01929f3beb96e058b8b4b0e3fe52d84f054c0e2a7a83".HexToByteArray());

        [Fact]
        [Trait("Category", "Unit")]
        public async void ShouldSignCorrectly()
        {
            await Task.Yield();
            string privateKey = "0xa07cb7889ab3a164dcc72cb6103f2573c7ef2d4a855810594d2bf25df60bc39e";

            PlasmaCore.Transactions.Transaction transaction = new PlasmaCore.Transactions.Transaction();
            transaction.AddInput(2000, 0, 1);
            transaction.AddOutput("0xf86b5b1c2c8de1ea4dc737c849272340fa3561c5", "0x0000000000000000000000000000000000000000", 123);
            transaction.AddOutput("0xf86b5b1c2c8de1ea4dc737c849272340fa3561c5", "0x0000000000000000000000000000000000000000", 5555308);

            TypedDataTransactionEncoder txEncoder = new TypedDataTransactionEncoder(omgDomain);
            byte[] encodedTx = txEncoder.EncodeRaw(transaction);
            var rawHash = new Sha3Keccack().CalculateHash(encodedTx);
            var ecKey = new Nethereum.Signer.EthECKey(privateKey);
            var ecdsaSignature = ecKey.SignAndCalculateV(rawHash);

            string signature = Nethereum.Signer.EthECDSASignature.CreateStringSignature(ecdsaSignature);
            Assert.Equal("0xed0ff5633cb85aa0f64684759185f8a9f94fd1b654be5942d562bf64f504e3a96a83b90a5e50e50b8a75d4f711d1c0e56066519237dbd94e564084a561b8ba2f1b",
                signature.EnsureHexPrefix());

            transaction.SetSignature(0, signature.HexToByteArray());

            var signedEncodedTx = txEncoder.EncodeSigned(transaction).ToHex(true);
            Assert.Equal("0xf9012ef843b841ed0ff5633cb85aa0f64684759185f8a9f94fd1b654be5942d562bf64f504e3a96a83b90a5e50e50b8a75d4f711d1c0e56066519237dbd94e564084a561b8ba2f1bd2c58207d08001c3808080c3808080c3808080f8b3eb94f86b5b1c2c8de1ea4dc737c849272340fa3561c59400000000000000000000000000000000000000007bee94f86b5b1c2c8de1ea4dc737c849272340fa3561c59400000000000000000000000000000000000000008354c46ceb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080eb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080a00000000000000000000000000000000000000000000000000000000000000000",
                signedEncodedTx.EnsureHexPrefix());
        }
    }
}
