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
                Debug.WriteLine("\tName: " + hoard.DefaultGame.Name);
                Debug.WriteLine("\tBackend Url: " + hoard.DefaultGame.Url);
                Debug.WriteLine("\tGameID: " + hoard.DefaultGame.ID);
            }

            //Hoard.PlayerID myId = new PlayerID("0x5d0774af3a8f7656dc61bcf30e383056275911b7","");
            //Assert.True(myId != PlayerID.kInvalidID, "ERROR: Invalid player ID!");
            //Debug.WriteLine(string.Format("Current player is: {0}", myId.ID));

            GameID myGame = new GameID("mygame");

            hoard.RegisterGame(myGame, new CKGameItemProvider(myGame));

            GameItem[] items = hoard.GetPlayerItems(HoardService.Instance.DefaultUser, myGame);
            
            Debug.WriteLine("Shutting down HOARD...");
            Assert.True(hoard.Shutdown());
        }
    }
}
