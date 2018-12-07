using Hoard;
using HoardTests.Fixtures;
using System;
using System.Diagnostics;
using Xunit;
using static Hoard.KeyStoreAccountService;

namespace HoardTests
{
    public class HoardExchangeTests : IClassFixture<HoardServiceFixture>, IClassFixture<HoardExchangeFixture>
    {
        static string HoardGameTestName = "HoardExchange";

        public HoardService HoardService { get; private set; }

        public HoardExchangeTests(HoardServiceFixture _hoardService, HoardExchangeFixture _hoardExchange)
        {
            _hoardService.Initialize(HoardGameTestName);
            HoardService = _hoardService.HoardService;

            var user = new User("user");
            var account = new KeyStoreAccount("keystore", "0xdc457d26e6c34ed3c3db13e9af63709869bc3565", "0x28a9e620b91445c666b91d9c7eba7ba109289a11fdbbad9c7e2812538239e9e6");
            user.Accounts.Add(account);
            user.SetActiveAccount(account);
            HoardService.DefaultUser = user;

            _hoardExchange.Initialize(HoardService);
            ((HoardExchangeService)HoardService.ExchangeService).SetUser(user);
        }

        [Fact]
        public void ListAllOrders()
        {
            var orders = HoardService.ExchangeService.ListOrders(null, null).Result;
            Assert.NotEmpty(orders);
        }
    }
}
