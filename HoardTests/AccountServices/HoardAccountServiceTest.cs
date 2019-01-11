using Hoard;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.AccountServices
{
    public class HoardAccountServiceTest
    {
        public class UserInputProviderFixture : IUserInputProvider
        {
            public async Task<string> RequestInput(User user, eUserInputType type, string description)
            {
                if (type == eUserInputType.kLogin)
                    return "TestUser";
                else if (type == eUserInputType.kPassword)
                    return "password";

                return null;
            }
        }

        IAccountService signer;
        User user;

        public HoardAccountServiceTest()
        {
            signer = new HoardAccountService("http://localhost:8081", "wss://localhost:8082", "HoardTestAuthClient", new UserInputProviderFixture());
            user = new User("rafalw", "rafal.wydra@gmail.com");
            bool res = signer.RequestAccounts(user).Result;
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SignMessage()
        {
            var message = "Hello world";
            var signature = await signer.SignMessage(message.ToBytesForRLPEncoding(), user.ActiveAccount);

            var msgSigner = new EthereumMessageSigner();
            var addressRec = new HoardID(msgSigner.EncodeUTF8AndEcRecover(message, signature));
            Assert.Equal(user.Accounts[0].ID, addressRec);
        }
    }
}
