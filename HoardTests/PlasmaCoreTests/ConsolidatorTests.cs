using HoardTests.Fixtures;
using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore;
using PlasmaCore.UTXO;
using System;
using System.Linq;
using Xunit;

namespace HoardTests.PlasmaCoreTests
{
    public class ConsolidatorTests : IClassFixture<MockupPlasmaAPIServiceFixture>
    {
        public PlasmaAPIService PlasmaAPIService { get; private set; }

        public ConsolidatorTests(MockupPlasmaAPIServiceFixture plasmaAPIServiceFixture)
        {
            PlasmaAPIService = plasmaAPIServiceFixture.PlasmaAPIService;
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void ShouldConsolidateFungibleCurrencyTransactions()
        {
            string address = "0x0e5e50883f3a1dd73c170c935339bce1b24a96d0";
            string privateKey = "0xac32ae83a4067291cda7a268e316376338bef6b63f66d10b8b516c76093c677e";
            string currency = "0xda636e31a9800531418213b5c799960f4585c937";

            var utxos = await PlasmaAPIService.GetUtxos(address);
            var consolidator = new FCConsolidator(PlasmaAPIService, address, currency, utxos, 4);
            while (consolidator.CanMerge)
            {
                foreach (var transaction in consolidator.Transactions)
                {
                    var sig = PlasmaCoreTestsHelper.Sign(transaction.GetRLPEncodedRaw(), privateKey);
                    transaction.SetSignature(address, sig.HexToByteArray());
                }
                await consolidator.ProcessTransactions();
            }

            Assert.Equal(4, (consolidator.MergedUtxo as FCUTXOData).Amount);
            Assert.Equal(true, consolidator.AllConsolidated);
            Assert.Equal(!consolidator.CanMerge, consolidator.AllConsolidated);


            consolidator = new FCConsolidator(PlasmaAPIService, address, currency, utxos);
            while (consolidator.CanMerge)
            {
                foreach (var transaction in consolidator.Transactions)
                {
                    var sig = PlasmaCoreTestsHelper.Sign(transaction.GetRLPEncodedRaw(), privateKey);
                    transaction.SetSignature(address, sig.HexToByteArray());
                }
                await consolidator.ProcessTransactions();
            }

            Assert.Equal(1744, (consolidator.MergedUtxo as FCUTXOData).Amount);
            Assert.Equal(true, consolidator.AllConsolidated);
            Assert.Equal(!consolidator.CanMerge, consolidator.AllConsolidated);


            consolidator = new FCConsolidator(PlasmaAPIService, address, currency, utxos, 9999999);
            while (consolidator.CanMerge)
            {
                foreach (var transaction in consolidator.Transactions)
                {
                    var sig = PlasmaCoreTestsHelper.Sign(transaction.GetRLPEncodedRaw(), privateKey);
                    transaction.SetSignature(address, sig.HexToByteArray());
                }
                await consolidator.ProcessTransactions();
            }

            Assert.Equal(1744, (consolidator.MergedUtxo as FCUTXOData).Amount);
            Assert.Equal(true, consolidator.AllConsolidated);
            Assert.Equal(!consolidator.CanMerge, consolidator.AllConsolidated);
        }
    }
}
