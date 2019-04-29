using Hoard;
using Hoard.ExchangeServices;
using HoardTests.Fixtures;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Xunit;

namespace HoardTests
{
    [TestCaseOrderer("HoardTests.TestCaseOrdering.PriorityOrderer", "HoardTests")]
    public class HoardExchangeTests : IClassFixture<HoardExchangeFixture>
    {
        public HoardExchangeFixture HoardExchangeFixture { get; private set; }
        public HoardService HoardService { get; private set; }
        public GameID[] gameIDs { get; private set; }
        public Profile[] users { get; private set; }
        public List<GameItem> items { get; private set; }

        public BCExchangeService BCExchangeService { get; private set; }
        public HoardExchangeService HoardExchangeService { get; private set; }

        public HoardExchangeTests(HoardExchangeFixture _hoardExchange)
        {
            HoardExchangeFixture = _hoardExchange;
            HoardService = _hoardExchange.HoardService;

            BCExchangeService = _hoardExchange.BCExchangeService;
            HoardExchangeService = _hoardExchange.HoardExchangeService;

            gameIDs = _hoardExchange.GameIDs;
            users = _hoardExchange.Users;
            items = _hoardExchange.Items;
        }

        [Fact, TestPriority(0)]
        public void ListAllOrders()
        {
            var orders = ListOrders(null, null, null);
            Assert.NotEmpty(orders);
        }

        [Fact, TestPriority(1)]
        public void Deposit()
        {
            //ERC223
            Profile account = users[0];
            var success = BCExchangeService.Deposit(account, items[1], 1).Result;
            Assert.True(success);

            //deposit non-existing items
            success = BCExchangeService.Deposit(account, items[1], 1).Result;
            Assert.False(success);

            account = users[1];
            success = BCExchangeService.Deposit(account, items[0], 1000).Result;
            Assert.True(success);

            //ERC721
            success = BCExchangeService.Deposit(account, items[2], 1).Result;
            Assert.True(success);
        }

        [Fact, TestPriority(2)]
        public void Order()
        {
            Profile account = users[0];

            //ERC223
            items[0].Metadata.Set<BigInteger>("Balance", 20);
            var success = BCExchangeService.Order(account, items[0], items[1], 0xFFFFFFF).Result;
            Assert.True(success);

            Thread.Sleep(3000);

            var orders = ListOrders(items[0], items[1], users[0]);
            Assert.NotEmpty(orders);

            //ERC721
            items[0].Metadata.Set<BigInteger>("Balance", 20);
            success = BCExchangeService.Order(account, items[0], items[2], 0xFFFFFFF).Result;
            Assert.True(success);

            Thread.Sleep(3000);

            orders = ListOrders(items[0], items[2], users[0]);
            Assert.NotEmpty(orders);
        }

        [Fact, TestPriority(3)]
        public void Trade()
        {
            Profile account = users[1];

            //ERC223
            var orders = ListOrders(items[0], items[1], users[0]);
            Assert.NotEmpty(orders);

            orders[0].amount = orders[0].amountGet / 2;
            var success = BCExchangeService.Trade(account, orders[0]).Result;
            Assert.False(success);

            orders[0].amount = orders[0].amountGet;
            success = BCExchangeService.Trade(account, orders[0]).Result;
            Assert.True(success);

            //trade non-existing order
            success = BCExchangeService.Trade(account, orders[0]).Result;
            Assert.False(success);

            //ERC721
            orders = ListOrders(items[0], items[2], users[0]);
            Assert.NotEmpty(orders);

            orders[0].amount = orders[0].amountGet / 2;
            success = BCExchangeService.Trade(account, orders[0]).Result;
            Assert.False(success);

            orders[0].amount = orders[0].amountGet;
            success = BCExchangeService.Trade(account, orders[0]).Result;
            Assert.True(success);
        }

        [Fact, TestPriority(4)]
        public void Withdraw()
        {
            //ERC721
            Profile account = users[1];
            var success = BCExchangeService.Withdraw(account, items[2]).Result;
            Assert.True(success);

            //withdraw non-existing tokens
            success = BCExchangeService.Withdraw(account, items[2]).Result;
            Assert.False(success);

            //ERC223
            success = BCExchangeService.Withdraw(account, items[1]).Result;
            Assert.True(success);

            account = users[0];

            items[0].Metadata.Set<BigInteger>("Balance", 40);
            success = BCExchangeService.Withdraw(account, items[0]).Result;
            Assert.True(success);

            var items0 = HoardExchangeFixture.GetGameItems(users[0]).Result;
            Assert.Single(items0);

            var items1 = HoardExchangeFixture.GetGameItems(users[1]).Result;
            Assert.Equal(4, items1.Count);
        }

        [Fact, TestPriority(5)]
        public void CancelOrder()
        {
            Profile account  = users[1];

            //ERC223
            items[0].Metadata.Set<BigInteger>("Balance", 20);
            var success = BCExchangeService.Order(account, items[0], items[1], 0xFFFFFFF).Result;
            Assert.True(success);

            //ERC721
            items[0].Metadata.Set<BigInteger>("Balance", 20);
            success = BCExchangeService.Order(account, items[0], items[2], 0xFFFFFFF).Result;
            Assert.True(success);

            Thread.Sleep(3000);

            var orders = ListOrders(items[0], items[1], users[1]);
            Assert.True(orders.Length == 2);

            success = BCExchangeService.CancelOrder(account, orders[0]).Result;
            Assert.True(success);

            Thread.Sleep(3000);

            //FIXME is it correct?
            //cancel non-existing order
            success = BCExchangeService.CancelOrder(account, orders[0]).Result;
            Assert.True(success);

            Thread.Sleep(3000);

            orders = ListOrders(items[0], items[1], users[1]);
            Assert.True(orders.Length == 1);

            orders = ListOrders(items[0], items[2], users[1]);
            Assert.True(orders.Length == 1);

            success = BCExchangeService.CancelOrder(account, orders[0]).Result;
            Assert.True(success);

            Thread.Sleep(3000);
            
            orders = ListOrders(items[0], items[2], users[1]);
            Assert.True(orders.Length == 0);
        }

        //-----------------------------------------

        private Order[] ListOrders(GameItem itemGet, GameItem itemGive, Profile profile)
        {
            return HoardExchangeService.ListOrders(itemGet, itemGive, profile).Result;
        }
    }
}
