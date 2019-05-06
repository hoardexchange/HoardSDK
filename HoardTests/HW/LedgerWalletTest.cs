using Hoard;
using Hoard.HW;
using Hoard.HW.Ledger;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.HW
{
    public class LedgerWalletTest
    {
        LedgerWallet signer;

        public LedgerWalletTest()
        {
            signer = LedgerFactory.GetLedgerWalletAsync(DerivationPath.DefaultBIP44).Result;
            Assert.True(signer != null);
        }

        [Fact]
        public async Task DisplayAddress()
        {
            var user = await signer.RequestProfile("LedgerUser");
            Assert.True(user != null);
            Assert.True(user.Name == LedgerWallet.AccountInfoName);
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
                var user = await signer.RequestProfile("LedgerUser");
                var msgSigner = new EthereumMessageSigner();
                var addressRec = new HoardID(msgSigner.EcRecover(messages[i], signature));
                Assert.Equal(user.ID, addressRec);
            }
        }

        [Fact]
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

            var rlpEncoded = await signer.SignTransaction(rlpEncodedTransaction, null);
            Assert.True(rlpEncoded != null);
            Assert.True(rlpEncoded.Length > 0);

            var user = await signer.RequestProfile("LedgerUser");

//<<<<<<< HEAD
//            var decodedRlpEncoded = RLP.Decode(rlpEncoded.HexToByteArray());
//            var decodedRlpCollection = (RLPCollection)decodedRlpEncoded[0];

//            var signature = EthECDSASignatureFactory.FromComponents(
//                decodedRlpCollection[txEncoded.Count+1].RLPData,
//                decodedRlpCollection[txEncoded.Count+2].RLPData,
//                decodedRlpCollection[txEncoded.Count].RLPData
//            );

//            var rawHash = new Sha3Keccack().CalculateHash(rlpEncodedTransaction);

//            var account = new HoardID(EthECKey.RecoverFromSignature(signature, rawHash).GetPublicAddress());
//            Assert.Equal(user.ID, account);
//=======
//            tx = new RLPSigner(rlpEncoded.HexToByteArray(), 6);
//            var account = new HoardID(EthECKey.RecoverFromSignature(tx.Signature, tx.RawHash).GetPublicAddress());
//            Assert.Equal(user.ID, account);
//            Assert.Equal(tx.Data[3].ToHex().ToLower().EnsureHexPrefix(), "0x4bc1EF56d94c766A49153A102096E56fAE2004e1".ToLower());
//>>>>>>> development
        }
    }
}
