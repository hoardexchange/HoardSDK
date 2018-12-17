using Hoard;
using Hoard.HW;
using Hoard.HW.Ledger;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.HW
{
    public class LedgerWalletTest
    {
        IAccountService signer;

        public LedgerWalletTest()
        {
            signer = LedgerFactory.GetLedgerWalletAsync(DerivationPath.BIP44).Result;
            Assert.True(signer != null);
        }

        [Fact]
        public async Task DisplayAddress()
        {
            var user = new User("LedgerUser");
            var response = await signer.RequestAccounts(user);
            Assert.True(response);
            Assert.True(user.Accounts.Count > 0);
            Assert.True(user.Accounts[0].Name == LedgerWallet.AccountInfoName);
        }

        [Fact]
        public async Task SignMessages()
        {
            var rand = new Random();
            var messages = new List<byte[]>();

            var message0 = Encoding.UTF8.GetBytes("Hello world");
            var message1 = new byte[256];
            for (var i = 0; i < message1.Length; ++i)
                message1[i] = (byte)rand.Next(0, 256);
            var message2 = new byte[1000];
            for (var i = 0; i < message2.Length; ++i)
                message2[i] = (byte)rand.Next(0, 256);

            messages.Add(message0);
            messages.Add(message1);
            messages.Add(message2);

            for(var i = 0; i < messages.Count; ++i)
            {
                var signature = await signer.SignMessage(messages[i], null);

                var user = new User("LedgerUser");
                var response = await signer.RequestAccounts(user);

                var msgSigner = new EthereumMessageSigner();
                var addressRec = new HoardID(msgSigner.EcRecover(messages[i], signature));
                Assert.Equal(user.Accounts[0].ID, addressRec);
            }
        }

        [Fact]
        public async Task SignTransaction()
        {
            var nonce = 324.ToBytesForRLPEncoding();
            var gasPrice = 10000000000000.ToBytesForRLPEncoding();
            var startGas = 21000.ToBytesForRLPEncoding();
            var to = "0x4bc1EF56d94c766A49153A102096E56fAE2004e1".HexToByteArray();
            var value = 10000.ToBytesForRLPEncoding();
            var data = "".HexToByteArray();

            var txData = new byte[][] { nonce, gasPrice, startGas, to, value, data };
            var tx = new RLPSigner(txData);
            
            var rlpEncoded = await signer.SignTransaction(tx.GetRLPEncodedRaw(), null);
            Assert.True(rlpEncoded != null);
            Assert.True(rlpEncoded.Length > 0);

            var user = new User("LedgerUser");
            var response = await signer.RequestAccounts(user);

            tx = new RLPSigner(rlpEncoded.HexToByteArray(), 6);
            var account = new HoardID(EthECKey.RecoverFromSignature(tx.Signature, tx.RawHash).GetPublicAddress());
            Assert.Equal(user.Accounts[0].ID, account);
            Assert.Equal(tx.Data[3].ToHex().ToLower().EnsureHexPrefix(), "0x4bc1EF56d94c766A49153A102096E56fAE2004e1".ToLower());
        }
    }
}
