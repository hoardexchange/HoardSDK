using HoardTests.Fixtures;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Util;
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
            string addressTo = "0xdd15a3ba1287a1069be49a6ebee9ebdb84eafd31";
            string currency = "0x3e967151f952ec2bef08107e108747f715bb8b70";

            var utxos = await PlasmaAPIService.GetUtxos(address);

            PlasmaCore.Transactions.Transaction tx = FCTransactionBuilder.Build(address, addressTo, utxos, new BigInteger(1), currency);

            byte[] encodedTx = tx.GetRLPEncodedRaw();
            Assert.Equal("0xf8cbd6c58207d08080c5820bb88080c3048080c58203e88001f8b2eb94dd15a3ba1287a1069be49a6ebee9ebdb84eafd31943e967151f952ec2bef08107e108747f715bb8b7001ed940e5e50883f3a1dd73c170c935339bce1b24a96d0943e967151f952ec2bef08107e108747f715bb8b7082d6cfeb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080eb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080",
                encodedTx.ToHex(true).ToLower());

            var signature = Sign(encodedTx, "0xac32ae83a4067291cda7a268e316376338bef6b63f66d10b8b516c76093c677e");
            tx.SetSignature(address, signature.HexToByteArray());

            byte[] signedEncodedTx = tx.GetRLPEncoded();
            Assert.Equal("0xf901daf9010cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cd6c58207d08080c5820bb88080c3048080c58203e88001f8b2eb94dd15a3ba1287a1069be49a6ebee9ebdb84eafd31943e967151f952ec2bef08107e108747f715bb8b7001ed940e5e50883f3a1dd73c170c935339bce1b24a96d0943e967151f952ec2bef08107e108747f715bb8b7082d6cfeb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080eb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080",
                signedEncodedTx.ToHex(true).ToLower());

            TransactionReceipt receipt = await PlasmaAPIService.SubmitTransaction(signedEncodedTx.ToHex(true));
            Assert.NotNull(receipt);

            PlasmaCore.Transactions.Transaction decodedRawTransaction = new PlasmaCore.Transactions.Transaction(encodedTx);
            Assert.Equal(encodedTx, decodedRawTransaction.GetRLPEncodedRaw());

            PlasmaCore.Transactions.Transaction decodedTransaction = new PlasmaCore.Transactions.Transaction(signedEncodedTx);
            Assert.Equal(signedEncodedTx, decodedTransaction.GetRLPEncoded());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void ShouldSendFungibleCurrencyTransaction()
        {
            string address = "0x0e5e50883f3a1dd73c170c935339bce1b24a96d0";
            string addressTo = "0xdd15a3ba1287a1069be49a6ebee9ebdb84eafd31";
            string currency = "0x3e967151f952ec2bef08107e108747f715bb8b70";

            var utxos = await PlasmaAPIService.GetUtxos(address);

            PlasmaCore.Transactions.Transaction tx = FCTransactionBuilder.Build(address, addressTo, utxos, new BigInteger(1), currency);

            byte[] encodedTx = tx.GetRLPEncodedRaw();
            var signature = Sign(encodedTx, "0xac32ae83a4067291cda7a268e316376338bef6b63f66d10b8b516c76093c677e");
            tx.SetSignature(address, signature.HexToByteArray());

            byte[] signedEncodedTx = tx.GetRLPEncoded();
            Assert.Equal("0xf901daf9010cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cb841200d2b4370aac48a5bd9b404224395e6a2395d634973cc7bdf36a2091382a2772670b839b769617e3d2bd992e0dfe6fd1e4515cce060afa5f4f47aec45e555f91cd6c58207d08080c5820bb88080c3048080c58203e88001f8b2eb94dd15a3ba1287a1069be49a6ebee9ebdb84eafd31943e967151f952ec2bef08107e108747f715bb8b7001ed940e5e50883f3a1dd73c170c935339bce1b24a96d0943e967151f952ec2bef08107e108747f715bb8b7082d6cfeb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080eb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080", signedEncodedTx.ToHex(true));

            TransactionReceipt receipt = await PlasmaAPIService.SubmitTransaction(signedEncodedTx.ToHex(true));
            Assert.NotNull(receipt);
        }

        private string Sign(byte[] encodedTx, string privateKey)
        {
            var rawHash = new Sha3Keccack().CalculateHash(encodedTx);
            var ecKey = new EthECKey(privateKey);
            var ecdsaSignature = ecKey.SignAndCalculateV(rawHash);
            return EthECDSASignature.CreateStringSignature(ecdsaSignature);
        }
    }
}
