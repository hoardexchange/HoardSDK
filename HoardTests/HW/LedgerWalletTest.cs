﻿using Hoard;
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
            var user = await signer.CreateProfile("0");
            Assert.True(user != null);
            var user2 = await signer.RequestProfile(user.Name);
            Assert.True(user == user2);
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
                var user = await signer.RequestProfile(signer.Name+"\0");
                var signature = await user.SignMessage(messages[i]);
                var addressRec = Hoard.Utils.Helper.RecoverHoardIdFromMessage(messages[i], signature);
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

            var user = await signer.RequestProfile(signer.Name+"\0");

            var signature = await user.SignTransaction(rlpEncodedTransaction);
            var account = Hoard.Utils.Helper.RecoverHoardIdFromTransaction(signature, rlpEncodedTransaction);

            Assert.Equal(user.ID, account);
        }
    }
}
