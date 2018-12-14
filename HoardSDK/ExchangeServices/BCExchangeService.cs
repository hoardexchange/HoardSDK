using Hoard;
using Hoard.BC.Contracts;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.ExchangeServices
{
    public class BCExchangeService : IExchangeService
    {
        private HoardService Hoard = null;
        private Hoard.BC.BCComm BCComm = null;
        private ExchangeContract ExchangeContract = null;

        public User User {get; set;}

        public BCExchangeService(HoardService hoard)
        {
            Hoard = hoard;
            BCComm = hoard.BCComm;
            User = hoard.DefaultUser;
        }

        public bool Init()
        {
            ExchangeContract = BCComm.GetGameExchangeContractAsync().Result;
            if (ExchangeContract == null)
                return false;
            return true;
        }

        public void Shutdown()
        {
            ExchangeContract = null;
        }

        private class GameItemId
        {
            public string Address = null;
            public string TokenId = null;

            public GameItemId(string address, string tokenId)
            {
                Address = address;
                TokenId = tokenId;
            }

            public override int GetHashCode()
            {
                return Address.GetHashCode() + TokenId.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                GameItemId itemId = obj as GameItemId;
                return Address == itemId.Address && TokenId == itemId.TokenId;
            }
        }

        public async Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive, AccountInfo account)
        {
            // FIXME: is it possible to get orders directly from bc?
            return new Order[0];
        }

        public async Task<bool> Trade(Order order)
        {
            if (order.gameItemGive.Metadata is ERC223GameItemContract.Metadata)
            {
                return await ExchangeContract.Trade(
                    User.ActiveAccount,
                    order.tokenGet,
                    order.amountGet,
                    order.tokenGive,
                    order.amountGive,
                    order.expires,
                    order.nonce,
                    order.user,
                    order.amount);
            }
            else if (order.gameItemGive.Metadata is ERC721GameItemContract.Metadata)
            {
                return await ExchangeContract.TradeERC721(
                    User.ActiveAccount,
                    order.tokenGet,
                    order.amountGet,
                    order.tokenGive,
                    order.tokenIdGive,
                    order.expires,
                    order.nonce,
                    order.user,
                    order.amount);
            }

            throw new NotImplementedException();
        }

        public async Task<bool> Order(GameItem getItem, GameItem giveItem, ulong blockTimeDuration)
        {
            if (giveItem.Metadata is ERC223GameItemContract.Metadata)
            {
                return await ExchangeContract.Order(
                    User.ActiveAccount,
                    getItem.Metadata.Get<string>("OwnerAddress"),
                    getItem.Metadata.Get<BigInteger>("Balance"),
                    giveItem.Metadata.Get<string>("OwnerAddress"),
                    giveItem.Metadata.Get<BigInteger>("Balance"),
                    blockTimeDuration);
            }
            else if (giveItem.Metadata is ERC721GameItemContract.Metadata)
            {
                return await ExchangeContract.OrderERC721(
                    User.ActiveAccount,
                    getItem.Metadata.Get<string>("OwnerAddress"),
                    getItem.Metadata.Get<BigInteger>("Balance"),
                    giveItem.Metadata.Get<string>("OwnerAddress"),
                    giveItem.Metadata.Get<BigInteger>("ItemId"),
                    blockTimeDuration);
            }

            throw new NotImplementedException();
        }

        public async Task<bool> Deposit(GameItem item, ulong amount)
        {
            try
            {
                IGameItemProvider gameItemProvider = Hoard.GetGameItemProvider(item);
                if (gameItemProvider != null)
                {
                    return await gameItemProvider.Transfer(User.ActiveAccount.ID, ExchangeContract.Address, item, amount);
                }
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException ex)
            {
                // TODO: log invalid transaction
            }
            return false;
        }

        public async Task<bool> Withdraw(GameItem item)
        {
            try
            {
                if (item.Metadata is ERC223GameItemContract.Metadata)
                {
                    return await ExchangeContract.Withdraw(User.ActiveAccount,
                                                            item.Metadata.Get<string>("OwnerAddress"),
                                                            item.Metadata.Get<BigInteger>("Balance"));
                }
                else if (item.Metadata is ERC721GameItemContract.Metadata)
                {
                    return await ExchangeContract.WithdrawERC721(User.ActiveAccount,
                                                                item.Metadata.Get<string>("OwnerAddress"),
                                                                item.Metadata.Get<BigInteger>("ItemId"));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException ex)
            {
                // TODO: log invalid withdraw
            }
            return false;
        }

        public async Task<bool> CancelOrder(Order order)
        {
            if (order.gameItemGive.Metadata is ERC223GameItemContract.Metadata)
            {
                return await ExchangeContract.CancelOrder(
                    User.ActiveAccount,
                    order.tokenGet,
                    order.amountGet,
                    order.tokenGive,
                    order.amountGive,
                    order.expires,
                    order.nonce);
            }
            else if (order.gameItemGive.Metadata is ERC721GameItemContract.Metadata)
            {
                return await ExchangeContract.CancelOrderERC721(
                    User.ActiveAccount,
                    order.tokenGet,
                    order.amountGet,
                    order.tokenGive,
                    order.tokenIdGive,
                    order.expires,
                    order.nonce);
            }

            throw new NotImplementedException();
        }
    }
}
