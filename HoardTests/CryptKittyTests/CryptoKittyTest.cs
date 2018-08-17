using Hoard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.CryptKittyTests
{
    public class CryptoKittyTest
    {
        [Fact]
        public void TestCryptoKittyProvider()
        {
            HoardService hoard = HoardService.Instance;

            HoardServiceOptions options = new HoardServiceOptions();
            options.RpcClient = new Nethereum.JsonRpc.Client.RpcClient(new Uri(@"http://acedkewlxuu2nfnaexb4eraa.devel.hoard.exchange:8545"));
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

            Hoard.PlayerID myId = new PlayerID("0x5d0774af3a8f7656dc61bcf30e383056275911b7","");
            Assert.True(myId != PlayerID.kInvalidID, "ERROR: Invalid player ID!");
            Debug.WriteLine(string.Format("Current player is: {0}", myId.ID));

            GameID myGame = new GameID("mygame");

            hoard.RegisterGame(myGame, new CKGameItemProvider());

            GameItem[] items = hoard.GetPlayerItems(myId, myGame);
            
            Debug.WriteLine("Shutting down HOARD...");
            Assert.True(hoard.Shutdown());
        }
    }
}
