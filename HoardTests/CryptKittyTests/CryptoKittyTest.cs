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
                ErrorCallbackProvider.ReportInfo("\tName: " + hoard.DefaultGame.Name);
                ErrorCallbackProvider.ReportInfo("\tBackend Url: " + hoard.DefaultGame.Url);
                ErrorCallbackProvider.ReportInfo("\tGameID: " + hoard.DefaultGame.ID);
            }

            //Hoard.PlayerID myId = new PlayerID("0x5d0774af3a8f7656dc61bcf30e383056275911b7","");
            //Assert.True(myId != PlayerID.kInvalidID, "ERROR: Invalid player ID!");
            //ErrorCallbackProvider.ReportInfo(string.Format("Current player is: {0}", myId.ID));

            GameID myGame = GameID.FromName("mygame");

            Assert.True(hoard.RegisterGame(myGame, new CKGameItemProvider(myGame)).Result);

            GameItem[] items = hoard.GetPlayerItems(hoardFixture.UserIDs[0], myGame).Result;

            ErrorCallbackProvider.ReportInfo("Shutting down HOARD...");
            Assert.True(hoard.Shutdown());
        }
    }
}
