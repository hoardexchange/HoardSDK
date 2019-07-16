using Hoard;
using HoardTests.Fixtures;
using System;
using System.Numerics;
using Xunit;

namespace HoardTests.Plasma
{
    // TODO plasma test automation (contract deployment, token deposit)
    // Requires running ethereum with HoardGame.test.js test deployed and plasma network
    // Requires deposited eth and erc223 token on childchain for playerAccount
    // Please remember that erc223 token should be listed in fee_specs.json (elixir_omg file)
    [TestCaseOrderer("HoardTests.TestCaseOrdering.PriorityOrderer", "HoardTests")]
    public class HoardServiceTests : IClassFixture<HoardServiceFixture>
    {
        HoardServiceFixture hoardFixture;

        public HoardService HoardService { get; private set; }

        private Profile playerProfile = null;
        private GameID gameID = null;

        public HoardServiceTests(HoardServiceFixture _hoardFixture)
        {
            hoardFixture = _hoardFixture;
            hoardFixture.InitializeFromConfig();
            HoardService = hoardFixture.HoardService;

            Assert.True(HoardService.Options.BCClientOptions is PlasmaClientOptions);

            playerProfile = KeyStoreProfileService.CreateProfileDirect("keyStore", "0xea93fd741e8508d4f9a5039761496c31b742001e88b88c260c2a47105e329d37");

            gameID = new GameID(BigInteger.Parse("2c3257614189ee907c819a4c92b04c6b9e6e9346051563e780d3c302e67e76b1", System.Globalization.NumberStyles.AllowHexSpecifier));

            //HoardService.Shutdown();
        }

        [Fact, TestPriority(0)]
        public void RegisterHoardGame()
        {
            GameID[] games = HoardService.GetAllHoardGames().Result;
            Assert.NotEmpty(games);

            Assert.DoesNotContain(GameID.FromName("12345"), games);
            try
            {
                HoardService.RegisterHoardGame(GameID.FromName("12345"));
                Assert.Contains(gameID, games);
                HoardService.RegisterHoardGame(gameID);
            }
            catch (Exception)
            {
                Assert.True(false);
            }
        }

        [Fact, TestPriority(1)]
        public void GetBalanceERC223()
        {
            var balance = HoardService.GetBalance(playerProfile).Result;
            Assert.True(balance > 0);

            var hrdBalance = HoardService.GetHRDAmount(playerProfile).Result;
            Assert.True(hrdBalance == 0);

            var gameItems = HoardService.GetPlayerItems(playerProfile, gameID).Result;
            Assert.NotEmpty(gameItems);
            Assert.True(gameItems[0].Metadata.Get<BigInteger>("Balance") > 0);
        }

        [Fact, TestPriority(2)]
        public void GameItemTransfer()
        {
            var sender = playerProfile;
            var receiver = hoardFixture.CreateUser().Result;

            var gameItems = HoardService.GetPlayerItems(sender, gameID).Result;
            Assert.NotEmpty(gameItems);

            var startingAmount = gameItems[0].Metadata.Get<BigInteger>("Balance");
            var amount = new BigInteger(10);

            var success = HoardService.RequestGameItemTransfer(sender, receiver.ID, gameItems[0], amount).Result;
            Assert.True(success);

            var gameItems1 = HoardService.GetPlayerItems(receiver, gameID).Result;
            Assert.NotEmpty(gameItems1);
            Assert.Equal(gameItems1[0].Metadata.Get<BigInteger>("Balance"), amount);

            var gameItems2 = HoardService.GetPlayerItems(sender, gameID).Result;
            Assert.NotEmpty(gameItems2);
            Assert.Equal(gameItems2[0].Metadata.Get<BigInteger>("Balance"), startingAmount - amount);
        }
    }
}