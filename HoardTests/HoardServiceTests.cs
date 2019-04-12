using Hoard;
using Hoard.ExchangeServices;
using HoardTests.Fixtures;
using System;
using System.Diagnostics;
using Xunit;

namespace HoardTests
{
    public class HoardServiceTests : IClassFixture<HoardServiceFixture>
    {
        static string HoardGameTestName = "HoardGame";
        HoardServiceFixture hoardFixture;

        public HoardService HoardService { get; private set; }

        public HoardServiceTests(HoardServiceFixture _hoardFixture)
        {
            hoardFixture = _hoardFixture;
        }

        [Fact]
        public void RegisterHoardGame()
        {
            hoardFixture.Initialize(HoardGameTestName);
            HoardService = hoardFixture.HoardService;

            GameID[] games = HoardService.GetAllHoardGames().Result;
            Assert.NotEmpty(games);

            GameID gameID = GameID.FromName("12345");
            Assert.DoesNotContain(gameID, games);
            Assert.False(HoardService.RegisterHoardGame(gameID).Result == Result.Ok);

            gameID = new GameID(System.Numerics.BigInteger.Parse("2c3257614189ee907c819a4c92b04c6b9e6e9346051563e780d3c302e67e76b1", System.Globalization.NumberStyles.AllowHexSpecifier));
            Assert.Contains(gameID, games);
            Assert.True(HoardService.RegisterHoardGame(gameID).Result == Result.Ok);

            HoardService.Shutdown();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TestHoardGames()
        {
            hoardFixture.InitializeFromConfig();
            HoardService = hoardFixture.HoardService;

            //ulong amount = (ulong)HoardService.GetHRDAmount(HoardService.DefaultPlayer);

            if (HoardService.DefaultGame != GameID.kInvalidID)
            {
                ErrorCallbackProvider.ReportInfo("\tName: " + HoardService.DefaultGame.Name);
                ErrorCallbackProvider.ReportInfo("\tBackend Url: " + HoardService.DefaultGame.Url);
                ErrorCallbackProvider.ReportInfo("\tGameID: " + HoardService.DefaultGame.ID);
            }

            ErrorCallbackProvider.ReportInfo("Getting Hoard games...");

            GameID[] games = HoardService.GetAllHoardGames().Result;

            ErrorCallbackProvider.ReportInfo(string.Format("Found {0} Hoard games.", games.Length));

            foreach (GameID game in games)
            {
                //Register hoard provider for this gam
                ErrorCallbackProvider.ReportInfo(string.Format("Registering Hoard game {0}", game.Name));
                var success = HoardService.RegisterHoardGame(game).Result;

                ErrorCallbackProvider.ReportInfo(string.Format("Getting player items for game {0}", game.Name));
                GameItem[] items = HoardService.GetPlayerItems(hoardFixture.UserIDs[0], game).Result;

                ErrorCallbackProvider.ReportInfo(string.Format("Found {0} items.", items.Length));
                foreach (GameItem gi in items)
                {
                    //assume we need to populate properties
                    //TODO: if properties is not null we would need to compare state with some cached data and if there is mismatch update too
                    ErrorCallbackProvider.ReportInfo(string.Format("Getting properties for item {0}:{1}...", gi.Symbol, gi.State));
                    if (gi.Properties == null)
                        success = HoardService.FetchItemProperties(gi).Result;
                    //TODO: enumerate properties...
                }
            }

            // Check exchange
            IExchangeService exchange = HoardService.ExchangeService;
            if (exchange != null)
            {
                var orders = exchange.ListOrders(null, null, null).Result;
                ErrorCallbackProvider.ReportInfo(string.Format("Found {0} exchange orders.", orders.Length));
                foreach (Order order in orders)
                {
                    ErrorCallbackProvider.ReportInfo(string.Format("Order: Buy {0} {1} for {2} {3}.",
                        order.amountGive,
                        order.gameItemGive.Symbol,
                        order.amountGet,
                        order.gameItemGet.Symbol
                    ));
                }
                // test trade:
                /*if (orders.Length > 1)
                {
                    Order order = orders[0];
                    bool result = exchange.Deposit(order.gameItemGet, order.amountGet).Result;
                    result = exchange.Trade(order, order.amountGet).Result;
                    result = exchange.Withdraw(order.gameItemGive, order.amountGive).Result;
                }*/
            }

            ErrorCallbackProvider.ReportInfo("Shutting down HOARD...");

            HoardService.Shutdown();
        }
    }
}
