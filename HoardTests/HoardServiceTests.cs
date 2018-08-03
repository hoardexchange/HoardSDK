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
    public class HoardServiceTests
    {
        [Fact]
        public void TestHoardGames()
        {
            HoardService hoard = HoardService.Instance;

            HoardServiceOptions options = new HoardServiceOptions();
            options.RpcClient = new Nethereum.JsonRpc.Client.RpcClient(new Uri(@"http://acedkewlxuu2nfnaexb4eraa.devel.hoard.exchange:8545"));
            options.GameBackendUrl = "";//no override, use URL from hoard.GameBackendDesc
            options.AccountsDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hoard", "acounts");

            Debug.WriteLine("Initalizing HOARD...");
            Stopwatch sw = Stopwatch.StartNew();
            Assert.True(hoard.Initialize(options), "ERROR: Could not initialize HOARD!");
            sw.Stop();
            Debug.WriteLine(string.Format("HOARD connected [{0}ms]!", sw.ElapsedMilliseconds));
            if (hoard.DefaultGame != GameID.kInvalidID)
            {
                Debug.WriteLine("\tName: " + hoard.DefaultGame.Name);
                Debug.WriteLine("\tBackend Url: " + hoard.DefaultGame.Url);
                Debug.WriteLine("\tGameID: " + hoard.DefaultGame.ID);
            }

            Hoard.PlayerID myId = hoard.DefaultPlayer;
            Assert.True(myId != PlayerID.kInvalidID, "ERROR: Invalid player ID!");
            Debug.WriteLine(string.Format("Current player is: {0}",myId.ID));

            Debug.WriteLine("Getting Hoard games...");

            sw = Stopwatch.StartNew();
            GameID[] games = hoard.QueryHoardGames().Result;
            sw.Stop();

            Debug.WriteLine(string.Format("Found {0} Hoard games. [{0}ms]!", games.Length, sw.ElapsedMilliseconds));

            foreach (GameID game in games)
            {
                //Register hoard provider for this gam
                Debug.WriteLine(String.Format("Registering Hoard game {0}",game.Name));
                hoard.RegisterHoardGame(game);

                Debug.WriteLine(String.Format("Getting player items for game {0}", game.Name));
                GameItem[] items = hoard.GetPlayerItems(hoard.DefaultPlayer, game);

                Debug.WriteLine(String.Format("Found {0} items.", items.Length));
                foreach (GameItem gi in items)
                {
                    //assume we need to populate properties
                    //TODO: if properties is not null we would need to compare state with some cached data and if there is mismatch update too
                    Debug.WriteLine(String.Format("Getting properties for item {0}:{1}...", gi.Symbol, gi.State));
                    if (gi.Properties == null)
                        hoard.UpdateItemProperties(gi);
                    //TODO: enumerate properties...
                }
            }

            Debug.WriteLine("Shutting down HOARD...");
            Assert.True(hoard.Shutdown());
        }
    }
}
