using Hoard;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using Nethereum.Util;
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
            var signature = await signer.SignMessage(message.ToBytesForRLPEncoding(), user);
            
            var msgSigner = new EthereumMessageSigner();
            var addressRec = new HoardID(msgSigner.EncodeUTF8AndEcRecover(message, signature));
            Assert.Equal(user.ID, addressRec);
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

            var rlpEncoded = await signer.SignTransaction(rlpEncodedTransaction, user);
            Assert.True(rlpEncoded != null);
            Assert.True(rlpEncoded.Length > 0);

            var decodedRlpEncoded = RLP.Decode(rlpEncoded.HexToByteArray());
            var decodedRlpCollection = (RLPCollection)decodedRlpEncoded[0];

            var signature = EthECDSASignatureFactory.FromComponents(
                decodedRlpCollection[2].RLPData,
                decodedRlpCollection[3].RLPData,
                decodedRlpCollection[1].RLPData
            );

            var rawHash = new Sha3Keccack().CalculateHash(rlpEncodedTransaction);

            var account = new HoardID(EthECKey.RecoverFromSignature(signature, rawHash).GetPublicAddress());
            Assert.Equal(user.ID, account);
        }
    }
}
