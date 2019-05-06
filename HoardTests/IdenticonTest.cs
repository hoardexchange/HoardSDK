using Hoard;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests
{
    public class IdenticonTest
    {
        public IdenticonTest()
        {
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task TestIdenticon()
        {
            Profile PlayerAccount = KeyStoreProfileService.CreateProfileDirect(
                "TestPlayer",
                "0x2370fd033278c143179d81c5526140625662b8daa446c22ee2d73db3707e620c");

            Hoard.Utils.Identicon identicon = new Hoard.Utils.Identicon(PlayerAccount.ID, 8);
            int w = 64, h = w;
            byte[] iconData = identicon.GetImageDataRGB(w);

            Assert.Equal(66, iconData[0]);
            Assert.Equal(12, iconData[1]);
            Assert.Equal(179, iconData[2]);

            Assert.Equal(208, iconData[10404]);
            Assert.Equal(111, iconData[10405]);
            Assert.Equal(51, iconData[10406]);
        }
    }
}
