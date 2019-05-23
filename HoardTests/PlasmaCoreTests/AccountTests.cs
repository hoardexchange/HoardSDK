using HoardTests.Fixtures;
using PlasmaCore;
using PlasmaCore.RPC.OutputData;
using PlasmaCore.UTXO;
using System.Linq;
using System.Numerics;
using Xunit;

namespace HoardTests.PlasmaCoreTests
{
    public class AccountTests : IClassFixture<MockupPlasmaAPIServiceFixture>
    {
        public PlasmaAPIService PlasmaAPIService { get; private set; }

        public AccountTests(MockupPlasmaAPIServiceFixture plasmaAPIServiceFixture)
        {
            PlasmaAPIService = plasmaAPIServiceFixture.PlasmaAPIService;
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void ShouldDeserializeUTXOData()
        {
            string address = "0x0e5e50883f3a1dd73c170c935339bce1b24a96d0";
            var utxos = await PlasmaAPIService.GetUtxos(address);

            Assert.True(utxos.All(x => x is FCUTXOData));
            Assert.NotEmpty(utxos);
            Assert.True(utxos.All(x => x.Owner == address));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void ShouldDeserializeBalanceData()
        {
            string address = "0x0e5e50883f3a1dd73c170c935339bce1b24a96d0";
            var balance = await PlasmaAPIService.GetBalance(address);

            Assert.True(balance.All(x => x is FCBalanceData));
            Assert.NotEmpty(balance);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void ShouldBalanceEqual()
        {
            string address = "0x0e5e50883f3a1dd73c170c935339bce1b24a96d0";

            var utxos = await PlasmaAPIService.GetUtxos(address);
            var balance = await PlasmaAPIService.GetBalance(address);

            foreach (var data in balance)
            {
                BigInteger sum = BigInteger.Zero;
                foreach (var utxo in utxos)
                {
                    if (utxo.Currency == data.Currency)
                        sum += (utxo as FCUTXOData).Amount;
                }

                Assert.Equal(sum, (data as FCBalanceData).Amount);
            }
        }
    }
}
