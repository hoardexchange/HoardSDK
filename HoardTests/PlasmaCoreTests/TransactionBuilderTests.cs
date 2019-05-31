using HoardTests.Fixtures;
using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore;
using PlasmaCore.RPC.OutputData;
using PlasmaCore.Transactions;
using System.Numerics;
using Xunit;

namespace HoardTests.PlasmaCoreTests
{
    public class TransactionBuilderTests : IClassFixture<MockupPlasmaAPIServiceFixture>
    {
        public PlasmaAPIService PlasmaAPIService { get; private set; }

        public TransactionBuilderTests(MockupPlasmaAPIServiceFixture plasmaAPIServiceFixture)
        {
            PlasmaAPIService = plasmaAPIServiceFixture.PlasmaAPIService;
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void ShouldBuildValidFungibleCurrencyTransaction()
        {
            string address = "0x0e5e50883f3a1dd73c170c935339bce1b24a96d0";
            string privateKey = "0xac32ae83a4067291cda7a268e316376338bef6b63f66d10b8b516c76093c677e";

            string addressTo = "0xdd15a3ba1287a1069be49a6ebee9ebdb84eafd31";
            string currency = "0x3e967151f952ec2bef08107e108747f715bb8b70";

            var utxos = await PlasmaAPIService.GetUtxos(address);

            PlasmaCore.Transactions.Transaction tx = FCTransactionBuilder.Build(address, addressTo, utxos, new BigInteger(1), currency);

            byte[] encodedTx = tx.GetRLPEncodedRaw();
            Assert.Equal("0xf8cbd6c58207d08080c5820bb88080c3048080c58203e88001f8b2eb94dd15a3ba1287a1069be49a6ebee9ebdb84eafd31943e967151f952ec2bef08107e108747f715bb8b7001ed940e5e50883f3a1dd73c170c935339bce1b24a96d0943e967151f952ec2bef08107e108747f715bb8b7082d6cfeb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080eb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080",
                encodedTx.ToHex(true).ToLower());

            var signature = PlasmaCoreTestsHelper.Sign(encodedTx, privateKey);
            tx.SetSignature(address, signature.HexToByteArray());

            byte[] signedEncodedTx = tx.GetRLPEncoded();
            Assert.Equal("0xf901daf9010cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cd6c58207d08080c5820bb88080c3048080c58203e88001f8b2eb94dd15a3ba1287a1069be49a6ebee9ebdb84eafd31943e967151f952ec2bef08107e108747f715bb8b7001ed940e5e50883f3a1dd73c170c935339bce1b24a96d0943e967151f952ec2bef08107e108747f715bb8b7082d6cfeb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080eb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080",
                signedEncodedTx.ToHex(true).ToLower());

            // create transaction from rlp encoded data
            PlasmaCore.Transactions.Transaction decodedRawTransaction = new PlasmaCore.Transactions.Transaction(encodedTx);
            Assert.Equal(encodedTx, decodedRawTransaction.GetRLPEncodedRaw());

            PlasmaCore.Transactions.Transaction decodedTransaction = new PlasmaCore.Transactions.Transaction(signedEncodedTx);
            Assert.Equal(signedEncodedTx, decodedTransaction.GetRLPEncoded());


            // other currency
            currency = "0x3f83c7446190ae039c54506b0f65ea8ee790ee7e";
            tx = FCTransactionBuilder.Build(address, addressTo, utxos, new BigInteger(1), currency);

            encodedTx = tx.GetRLPEncodedRaw();
            Assert.Equal("0xf8c9d4c58298581901c58223283f01c3808080c3808080f8b2eb94dd15a3ba1287a1069be49a6ebee9ebdb84eafd31943f83c7446190ae039c54506b0f65ea8ee790ee7e01ed940e5e50883f3a1dd73c170c935339bce1b24a96d0943f83c7446190ae039c54506b0f65ea8ee790ee7e82d47deb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080eb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080",
                encodedTx.ToHex(true).ToLower());

            signature = PlasmaCoreTestsHelper.Sign(encodedTx, privateKey);
            tx.SetSignature(address, signature.HexToByteArray());

            signedEncodedTx = tx.GetRLPEncoded();
            Assert.Equal("0xf90151f886b8411841224d01b5aad18730b257ceaac891d00c2fee3a90b196b855660569f8f34708031d97603c2664b7580e19090e4037fa679aa262c4f62c771a88190110feb81cb8411841224d01b5aad18730b257ceaac891d00c2fee3a90b196b855660569f8f34708031d97603c2664b7580e19090e4037fa679aa262c4f62c771a88190110feb81cd4c58298581901c58223283f01c3808080c3808080f8b2eb94dd15a3ba1287a1069be49a6ebee9ebdb84eafd31943f83c7446190ae039c54506b0f65ea8ee790ee7e01ed940e5e50883f3a1dd73c170c935339bce1b24a96d0943f83c7446190ae039c54506b0f65ea8ee790ee7e82d47deb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080eb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080",
                signedEncodedTx.ToHex(true).ToLower());

            decodedRawTransaction = new PlasmaCore.Transactions.Transaction(encodedTx);
            Assert.Equal(encodedTx, decodedRawTransaction.GetRLPEncodedRaw());

            decodedTransaction = new PlasmaCore.Transactions.Transaction(signedEncodedTx);
            Assert.Equal(signedEncodedTx, decodedTransaction.GetRLPEncoded());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void ShouldSendFungibleCurrencyTransaction()
        {
            string address = "0x0e5e50883f3a1dd73c170c935339bce1b24a96d0";
            string privateKey = "0xac32ae83a4067291cda7a268e316376338bef6b63f66d10b8b516c76093c677e";

            string addressTo = "0xdd15a3ba1287a1069be49a6ebee9ebdb84eafd31";
            string currency = "0x3e967151f952ec2bef08107e108747f715bb8b70";

            var utxos = await PlasmaAPIService.GetUtxos(address);

            PlasmaCore.Transactions.Transaction tx = FCTransactionBuilder.Build(address, addressTo, utxos, new BigInteger(1), currency);

            byte[] encodedTx = tx.GetRLPEncodedRaw();
            var signature = PlasmaCoreTestsHelper.Sign(encodedTx, privateKey);
            tx.SetSignature(address, signature.HexToByteArray());

            byte[] signedEncodedTx = tx.GetRLPEncoded();
            Assert.Equal("0xf901daf9010cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cd6c58207d08080c5820bb88080c3048080c58203e88001f8b2eb94dd15a3ba1287a1069be49a6ebee9ebdb84eafd31943e967151f952ec2bef08107e108747f715bb8b7001ed940e5e50883f3a1dd73c170c935339bce1b24a96d0943e967151f952ec2bef08107e108747f715bb8b7082d6cfeb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080eb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080", signedEncodedTx.ToHex(true));

            TransactionReceipt receipt = await PlasmaAPIService.SubmitTransaction(signedEncodedTx.ToHex(true));
            Assert.NotNull(receipt);
        }
    }
}
