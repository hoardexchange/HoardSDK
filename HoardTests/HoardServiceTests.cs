using Hoard;
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

            GameID[] games = HoardService.QueryHoardGames().Result;
            Assert.NotEmpty(games);

            GameID gameID = new GameID("12345");
            Assert.DoesNotContain(gameID, games);
            Assert.False(HoardService.RegisterHoardGame(gameID));

            gameID = new GameID("2c3257614189ee907c819a4c92b04c6b9e6e9346051563e780d3c302e67e76b1");
            Assert.Contains(gameID, games);
            Assert.True(HoardService.RegisterHoardGame(gameID));

            HoardService.Shutdown();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TestHoardGames()
        {
            hoardFixture.InitializeFromConfig();
            HoardService = hoardFixture.HoardService;

            if (HoardService.DefaultGame != GameID.kInvalidID)
            {
                Debug.WriteLine("\tName: " + HoardService.DefaultGame.Name);
                Debug.WriteLine("\tBackend Url: " + HoardService.DefaultGame.Url);
                Debug.WriteLine("\tGameID: " + HoardService.DefaultGame.ID);
            }

            Hoard.PlayerID myId = HoardService.DefaultPlayer;
            Assert.True(myId != PlayerID.kInvalidID, "ERROR: Invalid player ID!");
            Debug.WriteLine(string.Format("Current player is: {0}", myId.ID));

            Debug.WriteLine("Getting Hoard games...");

            GameID[] games = HoardService.QueryHoardGames().Result;

            Debug.WriteLine(string.Format("Found {0} Hoard games.", games.Length));

            foreach (GameID game in games)
            {
                //Register hoard provider for this gam
                Debug.WriteLine(String.Format("Registering Hoard game {0}", game.Name));
                HoardService.RegisterHoardGame(game);

                Debug.WriteLine(String.Format("Getting player items for game {0}", game.Name));
                GameItem[] items = HoardService.GetPlayerItems(HoardService.DefaultPlayer, game);

                Debug.WriteLine(String.Format("Found {0} items.", items.Length));
                foreach (GameItem gi in items)
                {
                    //assume we need to populate properties
                    //TODO: if properties is not null we would need to compare state with some cached data and if there is mismatch update too
                    Debug.WriteLine(String.Format("Getting properties for item {0}:{1}...", gi.Symbol, gi.State));
                    if (gi.Properties == null)
                        HoardService.FetchItemProperties(gi);
                    //TODO: enumerate properties...
                }
            }

            Debug.WriteLine("Shutting down HOARD...");

            HoardService.Shutdown();
        }
    }
}
