using Hoard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests
{
    public class HoardServiceInitializationFixture : IDisposable
    {
        public HoardServiceInitializationFixture()
        {
            HoardService hoard = HoardService.Instance;

            HoardServiceConfig config = HoardServiceConfig.Load();
            HoardServiceOptions options = new HoardServiceOptions(config, new Nethereum.JsonRpc.Client.RpcClient(new Uri(config.ClientUrl)));

            options.Game = GameID.kInvalidID;

            Assert.True(hoard.Initialize(options), "ERROR: Could not initialize HOARD!");
        }

        public void Dispose()
        {
            Assert.True(HoardService.Instance.Shutdown());
        }
    }

    public class HoardServiceTests : IClassFixture<HoardServiceInitializationFixture>
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void CheckDefaultPlayerExists()
        {
            HoardService hoard = HoardService.Instance;

            PlayerID myId = hoard.DefaultPlayer;
            Assert.True(myId != PlayerID.kInvalidID, "ERROR: Invalid player ID!");
        }

        [Fact]
        //[Trait("Category", "Unit")] //TODO: this is not true in default setup
        public void QueryHoardGames_NonEmpty()
        {
            HoardService hoard = HoardService.Instance;

            GameID[] games = hoard.QueryHoardGames().Result;

            Assert.True(games.Length > 0, "ERROR: No games registered!");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void RegisterHoardGamesAndGetPlayerItems()
        {
            HoardService hoard = HoardService.Instance;

            PlayerID myId = hoard.DefaultPlayer;
            Assert.True(myId != PlayerID.kInvalidID, "ERROR: Invalid player ID!");

            GameID[] games = hoard.QueryHoardGames().Result;

            foreach (GameID game in games)
            {
                //Register hoard provider for this game
                bool ret = hoard.RegisterHoardGame(game);
                Assert.True(ret,"ERROR: Could not register game "+game.Name);

                GameItem[] items = hoard.GetPlayerItems(hoard.DefaultPlayer, game);

                foreach (GameItem gi in items)
                {
                    //assume we need to populate properties
                    //TODO: if properties is not null we would need to compare state with some cached data and if there is mismatch update too
                    Debug.WriteLine(string.Format("Getting properties for item {0}:{1}...", gi.Symbol, gi.State));
                    if (gi.Properties == null)
                    {
                        ret = hoard.FetchItemProperties(gi);
                        Assert.True(ret, string.Format("ERROR: could not get propeties for item {0}:{1}...", gi.Symbol, gi.State));
                    }
                    //TODO: enumerate properties...
                }
            }
        }
    }
}
