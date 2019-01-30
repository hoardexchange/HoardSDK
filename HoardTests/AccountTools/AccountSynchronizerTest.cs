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
            //AccountSynchronizer AccountSync = new AccountSynchronizer("ws://localhost:8546");
            //bool res = AccountSync.Initialize().Result;
            //if(res)
            //{
            //    string pin = AccountSync.GeneratePin();
            //    string filterFrom = AccountSync.RegisterMessageFilterFrom(pin).Result;
            //    string filterTo = AccountSync.RegisterMessageFilterTo(pin).Result;
            //    string confirmationPin = AccountSync.GeneratePin();
            //    string msg = AccountSync.SendConfirmationPin(pin, confirmationPin).Result;
            //    int i = 8;
            //    while (i > 0)
            //    {
            //        res = AccountSync.Update(filterFrom).Result;
            //        i--;
            //    }

            //    await AccountSync.UnregisterMessageFilterTo(filterTo);
            //    await AccountSync.UnregisterMessageFilterFrom(filterFrom);
            //}
            //await AccountSync.Shutdown();
        }
    }
}
