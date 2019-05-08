using Hoard;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.AccountTools
{
    /// <summary>
    ///  geth parameters
    ///  geth --identity "MyTestNetNode" --nodiscover --datadir "d:\hoard" --wsorigins="*" --ws --wsport "8546" --wsaddr 0.0.0.0 --networkid 15 --shh --rpc --rpcport "8545" --rpcapi personal,db,eth,net,web3,shh console
    /// </summary>
    public class AccountSynchronizerTest
    {
        private static readonly string NodeUrl = "ws://ws.eth-rpc.hoard.exchange";

        public AccountSynchronizerTest()
        {
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GeneratePin()
        {
            AccountSynchronizer AccountSync = new AccountSynchronizerKeeper(NodeUrl);
            bool res = AccountSync.Initialize().Result;
            for (int i = 0; i < 100; i++)
            {
                string pin = AccountSynchronizer.GeneratePin();
                Assert.Equal(8, pin.Length);
            }
            await AccountSync.Shutdown();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task TransferKey()
        {
            AccountSynchronizerKeeper AccountSyncKeeper = new AccountSynchronizerKeeper(NodeUrl);
            AccountSynchronizerApplicant AccountSyncApplicant = new AccountSynchronizerApplicant(NodeUrl);
            bool res = await AccountSyncKeeper.Initialize();
            res = AccountSyncApplicant.Initialize().Result;
            if (res)
            {
                string pin = AccountSynchronizer.GeneratePin();
                string filterFrom = await AccountSyncKeeper.RegisterMessageFilter(pin);
                string filterTo = await AccountSyncApplicant.RegisterMessageFilter(pin);
                string confirmationPin = AccountSynchronizer.GeneratePin();
                string msg = await AccountSyncApplicant.SendConfirmationPin(confirmationPin);
                while (true)
                {
                    await AccountSyncApplicant.ProcessMessage(filterTo);
                    await AccountSyncKeeper.ProcessMessage(filterFrom);
                    if (AccountSyncKeeper.ConfirmationPinReceived())
                    {
                        msg = await AccountSyncKeeper.GenerateEncryptionKey();
                        break;
                    }
                }

                int confirmation = 0;
                while (true)
                {
                    await AccountSyncApplicant.ProcessMessage(filterTo);
                    await AccountSyncKeeper.ProcessMessage(filterFrom);
                    confirmation = AccountSyncKeeper.GetConfirmationStatus();
                    if (confirmation != 0)
                    {
                        break;
                    }
                }
                if (confirmation == 1)
                {
                    string keyStoreData = "{'crypto':{'cipher':'aes-128-ctr','ciphertext':'8fe0507d2858178a8832c3b921f994ddb43d3ba727786841d3499b94fdcaaf90','cipherparams':{'iv':'fad9089caee2003792ce6fec6d74f399'},'kdf':'scrypt','mac':'0da29fcf2ccfa9327cd5bb2a5f7e2a4b4a01ab6ba61954b174fdeeae46b228ab','kdfparams':{'n':262144,'r':1,'p':8,'dklen':32,'salt':'472c9a8bb1898a8abacca45ebb560427621004914edb78dfed4f82163d7fd2a2'}},'id':'1543aac7-c474-4819-98ee-af104528a91f','address':'0x167ba0a6918321b69d5792022ccb99dbeeb0f49a','version':3}";
                    msg = await AccountSyncKeeper.EncryptAndTransferKeystore(keyStoreData);
                }
                while (true)
                {
                    await AccountSyncApplicant.ProcessMessage(filterTo);
                    await AccountSyncKeeper.ProcessMessage(filterFrom);
                    if (AccountSyncApplicant.IsKeyStoreReceived())
                    {
                        string data = AccountSyncApplicant.GetKeystoreReceivedData();
                        break;
                    }
                }

                await AccountSyncApplicant.UnregisterMessageFilter(filterTo);
                await AccountSyncKeeper.UnregisterMessageFilter(filterFrom);
            }
            await AccountSyncApplicant.Shutdown();
            await AccountSyncKeeper.Shutdown();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task TransferKeyWhisperJS()
        {
            //AccountSynchronizerKeeper AccountSyncKeeper = new AccountSynchronizerKeeper("ws://localhost:8546");
            //bool res = AccountSyncKeeper.Initialize().Result;
            //if (res)
            //{
            //    string msg = "";
            //    string pin = "12345678";// AccountSynchronizer.GeneratePin();
            //    string filterFrom = AccountSyncKeeper.RegisterMessageFilter(pin).Result;
            //    while (true)
            //    {
            //        AccountSyncKeeper.ProcessMessage();
            //        if (AccountSyncKeeper.ConfirmationPinReceived())
            //        {
            //            msg = AccountSyncKeeper.GenerateEncryptionKey().Result;
            //            break;
            //        }
            //        Thread.Sleep(1000);
            //    }

            //    int confirmation = 0;
            //    while (true)
            //    {
            //        AccountSyncKeeper.ProcessMessage();
            //        confirmation = AccountSyncKeeper.GetConfirmationStatus();
            //        if (confirmation != 0)
            //        {
            //            break;
            //        }
            //    }

            //    if (confirmation == 1)
            //    {
            //        string keyStoreData = "{'crypto':{'cipher':'aes-128-ctr','ciphertext':'8fe0507d2858178a8832c3b921f994ddb43d3ba727786841d3499b94fdcaaf90','cipherparams':{'iv':'fad9089caee2003792ce6fec6d74f399'},'kdf':'scrypt','mac':'0da29fcf2ccfa9327cd5bb2a5f7e2a4b4a01ab6ba61954b174fdeeae46b228ab','kdfparams':{'n':262144,'r':1,'p':8,'dklen':32,'salt':'472c9a8bb1898a8abacca45ebb560427621004914edb78dfed4f82163d7fd2a2'}},'id':'1543aac7-c474-4819-98ee-af104528a91f','address':'0x167ba0a6918321b69d5792022ccb99dbeeb0f49a','version':3}";
            //        msg = AccountSyncKeeper.EncryptAndTransferKeystore(keyStoreData).Result;
            //    }

            //    await AccountSyncKeeper.UnregisterMessageFilter(filterFrom);
            //}
            //await AccountSyncKeeper.Shutdown();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ReceiveKeyWhisperJS()
        {
            //AccountSynchronizerApplicant AccountSyncApplicant = new AccountSynchronizerApplicant(NodeUrl);
            //bool res = AccountSyncApplicant.Initialize().Result;
            //if (res)
            //{
            //    string pin = "12345678";// AccountSynchronizer.GeneratePin();
            //    string filterTo = AccountSyncApplicant.RegisterMessageFilter(pin).Result;
            //    string confirmationPin = "12345678";// AccountSynchronizer.GeneratePin();
            //    string msg = AccountSyncApplicant.SendConfirmationPin(confirmationPin).Result;
            //    while (true)
            //    {
            //        AccountSyncApplicant.ProcessMessage();
            //        if (AccountSyncApplicant.IsKeyStoreReceived())
            //        {
            //            string data = AccountSyncApplicant.GetKeystoreReceivedData();
            //            break;
            //        }
            //    }
            //    await AccountSyncApplicant.UnregisterMessageFilter(filterTo);
            //}
            //await AccountSyncApplicant.Shutdown();
        }
    }
}
