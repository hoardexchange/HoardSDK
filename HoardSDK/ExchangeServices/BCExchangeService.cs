using Hoard.BC.Contracts;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.ExchangeServices
{
    public class BCExchangeService : IExchangeService
    {
        private HoardService Hoard = null;
        private BC.BCComm BCComm = null;
        private ExchangeContract ExchangeContract = null;

        public BCExchangeService(HoardService hoard)
        {
            Hoard = hoard;
            BCComm = hoard.BCComm;
        }

        public bool Init()
        {
            ExchangeContract = BCComm.GetGameExchangeContractAsync().Result;
            if (ExchangeContract == null)
            {
                System.Diagnostics.Trace.TraceError("Cannot get proper GameExchange contract!");
                return false;
            }
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

        public async Task<bool> Trade(AccountInfo account, Order order)
        {
            if (order.gameItemGive.Metadata is ERC223GameItemContract.Metadata)
            {
                return await ExchangeContract.Trade(
                    account,
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
                    account,
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

        public async Task<bool> Order(AccountInfo account, GameItem getItem, GameItem giveItem, ulong blockTimeDuration)
        {
            if (giveItem.Metadata is ERC223GameItemContract.Metadata)
            {
                return await ExchangeContract.Order(
                    account,
                    getItem.Metadata.Get<string>("OwnerAddress"),
                    getItem.Metadata.Get<BigInteger>("Balance"),
                    giveItem.Metadata.Get<string>("OwnerAddress"),
                    giveItem.Metadata.Get<BigInteger>("Balance"),
                    blockTimeDuration);
            }
            else if (giveItem.Metadata is ERC721GameItemContract.Metadata)
            {
                return await ExchangeContract.OrderERC721(
                    account,
                    getItem.Metadata.Get<string>("OwnerAddress"),
                    getItem.Metadata.Get<BigInteger>("Balance"),
                    giveItem.Metadata.Get<string>("OwnerAddress"),
                    giveItem.Metadata.Get<BigInteger>("ItemId"),
                    blockTimeDuration);
            }

            throw new NotImplementedException();
        }

        public async Task<bool> Deposit(AccountInfo account, GameItem item, ulong amount)
        {
            try
            {
                IGameItemProvider gameItemProvider = Hoard.GetGameItemProvider(item);
                if (gameItemProvider != null)
                {
                    return await gameItemProvider.Transfer(account.ID, ExchangeContract.Address, item, amount);
                }
                System.Diagnostics.Trace.TraceWarning($"Cannot find GameItemProvider for item: {item.Symbol}!");
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }
            return false;
        }

        public async Task<bool> Withdraw(AccountInfo account, GameItem item)
        {
            try
            {
                if (item.Metadata is ERC223GameItemContract.Metadata)
                {
                    return await ExchangeContract.Withdraw(account,
                                                            item.Metadata.Get<string>("OwnerAddress"),
                                                            item.Metadata.Get<BigInteger>("Balance"));
                }
                else if (item.Metadata is ERC721GameItemContract.Metadata)
                {
                    return await ExchangeContract.WithdrawERC721(account,
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
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return false;
            }
        }

        public async Task<bool> CancelOrder(AccountInfo account, Order order)
        {
            if (order.gameItemGive.Metadata is ERC223GameItemContract.Metadata)
            {
                return await ExchangeContract.CancelOrder(
                   account,
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
                    account,
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
