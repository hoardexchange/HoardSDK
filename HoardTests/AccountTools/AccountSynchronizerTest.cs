using Hoard;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.AccountTools
{
    /// <summary>
    ///  geth parameters
    ///  geth --identity "MyTestNetNode" --nodiscover --datadir "d:\hoard" --wsorigins="*" --ws --wsport "8546" --networkid 15 --shh --rpc --rpcport "8545" --rpcapi personal,db,eth,net,web3,shh console
    /// </summary>
    public class AccountSynchronizerTest
    {
        public AccountSynchronizerTest()
        {
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GeneratePin()
        {
            //AccountSynchronizer AccountSync = new AccountSynchronizer("ws://localhost:8546");
            //bool res = AccountSync.Initialize().Result;
            //for (int i = 0; i < 100; i++)
            //{
            //    string pin = AccountSync.GeneratePin();
            //    Assert.Equal(8, pin.Length);
            //}
            //await AccountSync.Shutdown();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task TransferKey()
        {
            AccountSynchronizerKeeper AccountSyncKeeper = new AccountSynchronizerKeeper("ws://10.30.10.121:3046");
            AccountSynchronizerApplicant AccountSyncApplicant = new AccountSynchronizerApplicant("ws://10.30.10.121:3046");
            bool res = AccountSyncKeeper.Initialize().Result;
            res = AccountSyncApplicant.Initialize().Result;
            if (res)
            {
                string pin = AccountSyncKeeper.GeneratePin();
                string filterFrom = AccountSyncKeeper.RegisterMessageFilter(pin).Result;
                string filterTo = AccountSyncApplicant.RegisterMessageFilter(pin).Result;
                string confirmationPin = AccountSyncApplicant.GeneratePin();
                string msg = AccountSyncApplicant.SendConfirmationPin(confirmationPin).Result;
                int i = 8;
                while (i > 0)
                {
                    res = AccountSyncKeeper.Update(filterFrom).Result;
                    i--;
                }
                if (AccountSyncKeeper.ConfirmationPinReceived())
                {
                    msg = AccountSyncKeeper.GenerateEncryptionKey().Result;
                }
                i = 8;
                while (i > 0)
                {
                    res = AccountSyncApplicant.Update(filterTo).Result;
                    res = AccountSyncKeeper.Update(filterFrom).Result;
                    i--;
                }
                int confirmation = AccountSyncKeeper.GetConfirmationStatus();
                if (confirmation == 1)
                {
                    string keyStoreData = "{'crypto':{'cipher':'aes-128-ctr','ciphertext':'8fe0507d2858178a8832c3b921f994ddb43d3ba727786841d3499b94fdcaaf90','cipherparams':{'iv':'fad9089caee2003792ce6fec6d74f399'},'kdf':'scrypt','mac':'0da29fcf2ccfa9327cd5bb2a5f7e2a4b4a01ab6ba61954b174fdeeae46b228ab','kdfparams':{'n':262144,'r':1,'p':8,'dklen':32,'salt':'472c9a8bb1898a8abacca45ebb560427621004914edb78dfed4f82163d7fd2a2'}},'id':'1543aac7-c474-4819-98ee-af104528a91f','address':'0x167ba0a6918321b69d5792022ccb99dbeeb0f49a','version':3}";
                    msg = AccountSyncKeeper.EncryptAndTransferKeystore(keyStoreData).Result;
                }
                i = 8;
                while (i > 0)
                {
                    res = AccountSyncApplicant.Update(filterTo).Result;
                    i--;
                }
                if (AccountSyncApplicant.IsKeyStoreReceived())
                {
                    string data = AccountSyncApplicant.GetKeystoreReceivedData();
                }
                await AccountSyncApplicant.UnregisterMessageFilter(filterTo);
                await AccountSyncKeeper.UnregisterMessageFilter(filterFrom);
            }
            await AccountSyncApplicant.Shutdown();
            await AccountSyncKeeper.Shutdown();
        }
    }
}
