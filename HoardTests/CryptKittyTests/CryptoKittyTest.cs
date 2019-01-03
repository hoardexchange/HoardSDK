using Hoard;
using HoardTests.Fixtures;
using System.Diagnostics;
using Xunit;

namespace HoardTests.CryptKittyTests
{
    public class CryptoKittyTest : IClassFixture<HoardServiceFixture>
    {
        HoardServiceFixture hoardFixture;

        public CryptoKittyTest(HoardServiceFixture _hoardFixture)
        {
            hoardFixture = _hoardFixture;
        }

        [Fact]
        public void TestCryptoKittyProvider()
        {
            hoardFixture.InitializeFromConfig();
            HoardService hoard = hoardFixture.HoardService;

            if (hoard.DefaultGame != GameID.kInvalidID)
            {
                Trace.TraceInformation("\tName: " + hoard.DefaultGame.Name);
                Trace.TraceInformation("\tBackend Url: " + hoard.DefaultGame.Url);
                Trace.TraceInformation("\tGameID: " + hoard.DefaultGame.ID);
            }

            //Hoard.PlayerID myId = new PlayerID("0x5d0774af3a8f7656dc61bcf30e383056275911b7","");
            //Assert.True(myId != PlayerID.kInvalidID, "ERROR: Invalid player ID!");
            //Trace.TraceInformation(string.Format("Current player is: {0}", myId.ID));

            GameID myGame = new GameID("mygame");

            hoard.RegisterGame(myGame, new CKGameItemProvider(myGame));

            GameItem[] items = hoard.GetPlayerItems(hoardFixture.UserIDs[0], myGame).Result;
            
            Trace.TraceInformation("Shutting down HOARD...");
            Assert.True(hoard.Shutdown());
        }
    }
}
