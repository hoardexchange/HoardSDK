using Hoard;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.AccountServices
{
    public class KeyStoreAccountServiceTest //: IClassFixture<HoardServiceFixture>
    {
        public class UserInputProviderFixture : IUserInputProvider
        {
            public async Task<string> RequestInput(User user, eUserInputType type, string description)
            {
                if (type == eUserInputType.kLogin)
                    return "TestUser";
                else if (type == eUserInputType.kPassword)
                    return "dev";

                return null;
            }
        }

        IAccountService signer;
        User user;

        public KeyStoreAccountServiceTest()
        {
            signer = new KeyStoreAccountService(new UserInputProviderFixture());
            user = new User("KeyStoreUser");
            user.SetActiveAccount(signer.CreateAccount("KeyStoreAccount", user).Result);
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

            var txData = new byte[][] { nonce, gasPrice, startGas, to.ToHexByteArray(), value, data };
            var tx = new RLPSigner(txData);

            var rlpEncoded = await signer.SignTransaction(tx.GetRLPEncodedRaw(), user.ActiveAccount);
            Assert.True(rlpEncoded != null);
            Assert.True(rlpEncoded.Length > 0);

            tx = new RLPSigner(rlpEncoded.HexToByteArray(), txData.Length);
            var account = new HoardID(EthECKey.RecoverFromSignature(tx.Signature, tx.RawHash).GetPublicAddress());
            Assert.Equal(user.Accounts[0].ID, account);
            Assert.Equal(new HoardID(tx.Data[3].ToHex()), to);
        }
    }
}
