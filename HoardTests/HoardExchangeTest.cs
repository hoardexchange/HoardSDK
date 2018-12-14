using Hoard;
using Hoard.BC.Contracts;
using HoardTests.Fixtures;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Xunit;
using static Hoard.KeyStoreAccountService;

namespace HoardTests
{
    [TestCaseOrderer("HoardTests.TestCaseOrdering.PriorityOrderer", "HoardTests")]
    public class HoardExchangeTests : IClassFixture<HoardExchangeFixture>
    {
        public HoardExchangeFixture HoardExchangeFixture { get; private set; }
        public HoardService HoardService { get; private set; }
        public HoardExchangeService ExchangeService { get; private set; }
        public GameID[] gameIDs { get; private set; }
        public User[] users { get; private set; }
        public List<GameItem> items { get; private set; }

        public HoardExchangeTests(HoardExchangeFixture _hoardExchange)
        {
            HoardExchangeFixture = _hoardExchange;
            HoardService = _hoardExchange.HoardService;
            ExchangeService = _hoardExchange.ExchangeService;
            gameIDs = _hoardExchange.GameIDs;
            users = _hoardExchange.Users;
            items = _hoardExchange.Items;
        }

        [Fact, TestPriority(0)]
        public void ListAllOrders()
        {
            var orders = ExchangeService.ListOrders(null, null, null).Result;
            Assert.NotEmpty(orders);
        }

        [Fact, TestPriority(1)]
        public void Deposit()
        {
            //ERC223
            ExchangeService.SetUser(users[0]);
            var success = ExchangeService.Deposit(items[1], 1).Result;
            Assert.True(success);

            //deposit non-existing items
            success = ExchangeService.Deposit(items[1], 1).Result;
            Assert.False(success);

            ExchangeService.SetUser(users[1]);
            success = ExchangeService.Deposit(items[0], 1000).Result;
            Assert.True(success);

            //ERC721
            ExchangeService.SetUser(users[0]);
            success = ExchangeService.Deposit(items[2], 1).Result;
            Assert.True(success);
        }

        [Fact, TestPriority(2)]
        public void Order()
        {
            ExchangeService.SetUser(users[0]);

            //ERC223
            items[0].Metadata.Set<BigInteger>("Balance", 20);
            var success = ExchangeService.Order(items[0], items[1], 0xFFFFFFF).Result;
            Assert.True(success);

            Thread.Sleep(3000);

            var orders = ExchangeService.ListOrders(items[0], items[1], users[0].ActiveAccount).Result;
            Assert.NotEmpty(orders);

            //ERC721
            items[0].Metadata.Set<BigInteger>("Balance", 20);
            success = ExchangeService.Order(items[0], items[2], 0xFFFFFFF).Result;
            Assert.True(success);

            Thread.Sleep(3000);

            orders = ExchangeService.ListOrders(items[0], items[2], users[0].ActiveAccount).Result;
            Assert.NotEmpty(orders);
        }

        [Fact, TestPriority(3)]
        public void Trade()
        {
            ExchangeService.SetUser(users[1]);

            //ERC223
            var orders = ExchangeService.ListOrders(items[0], items[1], users[0].ActiveAccount).Result;
            Assert.NotEmpty(orders);

            orders[0].amount = orders[0].amountGet / 2;
            var success = ExchangeService.Trade(orders[0]).Result;
            Assert.False(success);

            orders[0].amount = orders[0].amountGet;
            success = ExchangeService.Trade(orders[0]).Result;
            Assert.True(success);

            //trade non-existing order
            success = ExchangeService.Trade(orders[0]).Result;
            Assert.False(success);

            //ERC721
            orders = ExchangeService.ListOrders(items[0], items[2], users[0].ActiveAccount).Result;
            Assert.NotEmpty(orders);

            orders[0].amount = orders[0].amountGet / 2;
            success = ExchangeService.Trade(orders[0]).Result;
            Assert.False(success);

            orders[0].amount = orders[0].amountGet;
            success = ExchangeService.Trade(orders[0]).Result;
            Assert.True(success);
        }

        [Fact, TestPriority(4)]
        public void Withdraw()
        {
            //ERC721
            ExchangeService.SetUser(users[1]);
            var success = ExchangeService.Withdraw(items[2]).Result;
            Assert.True(success);

            //withdraw non-existing tokens
            success = ExchangeService.Withdraw(items[2]).Result;
            Assert.False(success);

            //ERC223
            success = ExchangeService.Withdraw(items[1]).Result;
            Assert.True(success);

            ExchangeService.SetUser(users[0]);

            items[0].Metadata.Set<BigInteger>("Balance", 40);
            success = ExchangeService.Withdraw(items[0]).Result;
            Assert.True(success);

            var items0 = HoardExchangeFixture.GetGameItems(users[0]).Result;
            Assert.Single(items0);

            var items1 = HoardExchangeFixture.GetGameItems(users[1]).Result;
            Assert.Equal(4, items1.Count);
        }

        [Fact, TestPriority(5)]
        public void CancelOrder()
        {
            ExchangeService.SetUser(users[1]);

            //ERC223
            items[0].Metadata.Set<BigInteger>("Balance", 20);
            var success = ExchangeService.Order(items[0], items[1], 0xFFFFFFF).Result;
            Assert.True(success);

            //ERC721
            items[0].Metadata.Set<BigInteger>("Balance", 20);
            success = ExchangeService.Order(items[0], items[2], 0xFFFFFFF).Result;
            Assert.True(success);

            Thread.Sleep(3000);

            var orders = ExchangeService.ListOrders(items[0], items[1], users[1].ActiveAccount).Result;
            Assert.True(orders.Length == 2);

            success = ExchangeService.CancelOrder(orders[0]).Result;
            Assert.True(success);

            Thread.Sleep(3000);

            //FIXME is it correct?
            //cancel non-existing order
            success = ExchangeService.CancelOrder(orders[0]).Result;
            Assert.True(success);

            Thread.Sleep(3000);

            orders = ExchangeService.ListOrders(items[0], items[1], users[1].ActiveAccount).Result;
            Assert.True(orders.Length == 1);

            orders = ExchangeService.ListOrders(items[0], items[2], users[1].ActiveAccount).Result;
            Assert.True(orders.Length == 1);

            success = ExchangeService.CancelOrder(orders[0]).Result;
            Assert.True(success);

            Thread.Sleep(3000);
            
            orders = ExchangeService.ListOrders(items[0], items[2], users[1].ActiveAccount).Result;
            Assert.True(orders.Length == 0);
        }
    }
}
