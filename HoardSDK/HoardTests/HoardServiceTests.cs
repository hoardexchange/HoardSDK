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
            HoardService hoard = new HoardService();

            HoardServiceOptions options = new HoardServiceOptions();
            options.RpcClient = new Nethereum.JsonRpc.Client.RpcClient(new Uri(@"http://acedkewlxuu2nfnaexb4eraa.devel.hoard.exchange:8545"));
            options.GameBackendUrl = "";//no override, use URL from hoard.GameBackendDesc
            options.AccountsDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hoard", "acounts");

            Debug.WriteLine("Initalizing HOARD...");
            Stopwatch sw = Stopwatch.StartNew();
            Assert.True(hoard.Initialize(options), "ERROR: Could not initialize HOARD!");
            sw.Stop();
            Debug.WriteLine(string.Format("HOARD connected [{0}ms]!", sw.ElapsedMilliseconds));
            Debug.WriteLine("\tName: " + hoard.DefaultGameID.Name);
            Debug.WriteLine("\tBackend Url: " + hoard.DefaultGameID.Url);
            Debug.WriteLine("\tGameID: " + hoard.DefaultGameID.ID);

            Hoard.PlayerID myId = hoard.DefaultPlayer;
            Assert.True(myId != PlayerID.kInvalidID, "ERROR: Invalid player ID!");

            Debug.WriteLine("Getting Hoard games...");

            sw = Stopwatch.StartNew();
            GameID[] games = hoard.GetHoardGames();
            sw.Stop();

            Debug.WriteLine(string.Format("Found {0} Hoard games. [{0}ms]!", games.Length, sw.ElapsedMilliseconds));

            //TODO: comment this!
            foreach (GameID game in games)
            {
                //initialize BC support for each game
                hoard.RegisterHoardGameItems(game);

                GameItem[] items = hoard.GetPlayerItems(hoard.DefaultPlayer, game);

                foreach (GameItem gi in items)
                {
                    //assume we need to populate properties
                    //TODO: if properties is not null we would need to compare checksum with some cached data and if there is mismatch update too
                    if (gi.Properties == null)
                        hoard.UpdateItemProperties(gi);
                }
            }

            Assert.True(hoard.Shutdown());
        }
    }
}
