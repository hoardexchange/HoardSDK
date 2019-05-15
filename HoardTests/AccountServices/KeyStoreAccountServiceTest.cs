using Hoard;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.AccountServices
{
    public class KeyStoreAccountServiceTest //: IClassFixture<HoardServiceFixture>
    {
        public class UserInputProviderFixture : IUserInputProvider
        {
            public async Task<string> RequestInput(string name, HoardID id, eUserInputType type, string description)
            {
                if (type == eUserInputType.kLogin)
                    return "TestUser";
                else if (type == eUserInputType.kPassword)
                    return "dev";

                return null;
            }
        }

        IProfileService signer;
        Profile user;

        public KeyStoreAccountServiceTest()
        {
            signer = new KeyStoreProfileService(new UserInputProviderFixture());
            user = signer.CreateProfile("KeyStoreUser").Result;
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SignMessage()
        {
            var message = "Hello world";
            var byteMsg = message.ToBytesForRLPEncoding();
            var signature = await user.SignMessage(byteMsg);            
            HoardID account = Hoard.Utils.Helper.RecoverHoardId(byteMsg, signature);
            Assert.Equal(user.ID, account);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SignTransaction()
        {
            HoardID to = new HoardID("0x4bc1EF56d94c766A49153A102096E56fAE2004e1");
            var nonce = 324.ToBytesForRLPEncoding();
            var gasPrice = 10000000000000.ToBytesForRLPEncoding();
            var startGas = 21000.ToBytesForRLPEncoding();
            var value = 10000.ToBytesForRLPEncoding();
            var data = "".HexToByteArray();

            var txEncoded = new List<byte[]>();
            txEncoded.Add(RLP.EncodeElement(nonce));
            txEncoded.Add(RLP.EncodeElement(gasPrice));
            txEncoded.Add(RLP.EncodeElement(startGas));
            txEncoded.Add(RLP.EncodeElement(to.ToHexByteArray()));
            txEncoded.Add(RLP.EncodeElement(value));
            txEncoded.Add(RLP.EncodeElement(data));

            var rlpEncodedTransaction = RLP.EncodeList(txEncoded.ToArray());

            var rlpEncoded = await user.SignTransaction(rlpEncodedTransaction);

            HoardID account = Hoard.Utils.Helper.RecoverHoardId(rlpEncoded);

            Assert.Equal(user.ID, account);
        }
    }
}
