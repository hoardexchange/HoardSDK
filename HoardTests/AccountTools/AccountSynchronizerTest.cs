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
            //AccountSynchronizerKeeper AccountSyncKeeper = new AccountSynchronizerKeeper("ws://localhost:8546");
            //AccountSynchronizerApplicant AccountSyncApplicant = new AccountSynchronizerApplicant("ws://localhost:8546");
            //bool res = AccountSyncKeeper.Initialize().Result;
            //res = AccountSyncApplicant.Initialize().Result;
            //if (res)
            //{
            //    string pin = AccountSyncKeeper.GeneratePin();
            //    string filterFrom = AccountSyncKeeper.RegisterMessageFilter(pin).Result;
            //    string filterTo = AccountSyncApplicant.RegisterMessageFilter(pin).Result;
            //    string confirmationPin = AccountSyncApplicant.GeneratePin();
            //    string msg = AccountSyncApplicant.SendConfirmationPin(pin, confirmationPin).Result;
            //    int i = 8;
            //    while (i > 0)
            //    {
            //        res = AccountSyncKeeper.Update(filterFrom).Result;
            //        i--;
            //    }
            //    if (AccountSyncKeeper.ConfirmationPinReceived())
            //    {
            //        msg = AccountSyncKeeper.GenerateEncryptionKey(pin).Result;
            //    }
            //    i = 8;
            //    while (i > 0)
            //    {
            //        res = AccountSyncApplicant.Update(filterTo).Result;
            //        i--;
            //    }
            //    await AccountSyncApplicant.UnregisterMessageFilter(filterTo);
            //    await AccountSyncKeeper.UnregisterMessageFilter(filterFrom);
            //}
            //await AccountSyncApplicant.Shutdown();
            //await AccountSyncKeeper.Shutdown();
        }
    }
}
