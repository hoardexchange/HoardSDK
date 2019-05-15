using Hoard;
using Hoard.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.AccountServices
{
    public class HoardAccountServiceTest
    {
        public class UserInputProviderFixture : IUserInputProvider
        {
            public async Task<string> RequestInput(string name, HoardID id, eUserInputType type, string description)
            {
                if (type == eUserInputType.kLogin)
                    return "TestUser";
                else if (type == eUserInputType.kPassword)
                    return "password";

                return null;
            }
        }

        IProfileService signer;
        static Profile hoardAccountServiceTestUser = null;

        public HoardAccountServiceTest()
        {
            signer = new HoardProfileService("http://localhost:8081", "wss://localhost:8082", "HoardTestAuthClient", new UserInputProviderFixture());
            if (hoardAccountServiceTestUser == null)
            {
                hoardAccountServiceTestUser = signer.RequestProfile("hoard").Result;
            }
        }

        [Fact]
        //[Trait("Category", "Unit")]
        public async Task SignMessage()
        {
            var message = "Hello world";
            var byteMsg = message.ToBytesForRLPEncoding();
            var signature = await hoardAccountServiceTestUser.SignMessage(byteMsg);
            var addressRec = Helper.RecoverHoardId(byteMsg, signature);
            Assert.Equal(hoardAccountServiceTestUser.ID, addressRec);
        }

        [Fact]
        //[Trait("Category", "Unit")]
        public async Task SignTransaction()
        {
            HoardID to = new HoardID("0x4bc1EF56d94c766A49153A102096E56fAE2004e1");
            var nonce = 324.ToBytesForRLPEncoding();
            var gasPrice = 10000000000000.ToBytesForRLPEncoding();
            var startGas = 21000.ToBytesForRLPEncoding();
            var value = 10000.ToBytesForRLPEncoding();
            var data = "".HexToByteArray();
            var txData = new byte[][] { nonce, gasPrice, startGas, to.ToHexByteArray(), value, data };
            var tx = new RLPSigner(txData);
            var signature = await hoardAccountServiceTestUser.SignTransaction(tx.GetRLPEncodedRaw());
            var addressRec = Helper.RecoverHoardId(signature);
            Assert.Equal(hoardAccountServiceTestUser.ID, addressRec);
        }
    }
}
