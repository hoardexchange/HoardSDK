using Hoard.BC.Contracts;
using Hoard.Exceptions;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.ExchangeServices
{
    /// <summary>
    /// Implementation of IExchangeService that directly communicates with blockchain data
    /// </summary>
    public class BCExchangeService : IExchangeService
    {
        private HoardService Hoard = null;
        private BC.BCComm BCComm = null;
        private ExchangeContract ExchangeContract = null;

        /// <summary>
        /// Creates new instance of <see cref="BCExchangeService"/>
        /// </summary>
        /// <param name="hoard">Hoard service</param>
        public BCExchangeService(HoardService hoard)
        {
            Hoard = hoard;
            BCComm = (BC.BCComm)hoard.BCComm;
        }

        /// <inheritdoc/>
        public async Task Init()
        {
            ExchangeContract = await BCComm.GetHoardExchangeContract();
            if (ExchangeContract == null)
            {
                throw new HoardException("Cannot get proper GameExchange contract!");
            }
        }

        /// <summary>
        /// Destroys this instance
        /// </summary>
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

        /// <inheritdoc/>
        public async Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive, Profile profile)
        {
            // FIXME: is it possible to get orders directly from bc?
            return await Task.FromResult(new Order[0]);
        }

        /// <inheritdoc/>
        public async Task<bool> Trade(Profile profile, Order order)
        {
            if (order.gameItemGive.Metadata is ERC223GameItemContract.Metadata)
            {
                return await ExchangeContract.Trade(
                    profile,
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
                    profile,
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

        /// <inheritdoc/>
        public async Task<bool> Order(Profile profile, GameItem getItem, GameItem giveItem, ulong blockTimeDuration)
        {
            if (giveItem.Metadata is ERC223GameItemContract.Metadata)
            {
                return await ExchangeContract.Order(
                    profile,
                    getItem.Metadata.Get<string>("OwnerAddress"),
                    getItem.Metadata.Get<BigInteger>("Balance"),
                    giveItem.Metadata.Get<string>("OwnerAddress"),
                    giveItem.Metadata.Get<BigInteger>("Balance"),
                    blockTimeDuration);
            }
            else if (giveItem.Metadata is ERC721GameItemContract.Metadata)
            {
                return await ExchangeContract.OrderERC721(
                    profile,
                    getItem.Metadata.Get<string>("OwnerAddress"),
                    getItem.Metadata.Get<BigInteger>("Balance"),
                    giveItem.Metadata.Get<string>("OwnerAddress"),
                    giveItem.Metadata.Get<BigInteger>("ItemId"),
                    blockTimeDuration);
            }

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<bool> Deposit(Profile profile, GameItem item, BigInteger amount)
        {
            try
            {
                IGameItemProvider gameItemProvider = Hoard.GetGameItemProvider(item);
                if (gameItemProvider != null)
                {
                    return await gameItemProvider.Transfer(profile, ExchangeContract.Address, item, amount);
                }
                throw new HoardException($"Cannot find GameItemProvider for item: {item.Symbol}!");
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> Withdraw(Profile profile, GameItem item)
        {
            try
            {
                if (item.Metadata is ERC223GameItemContract.Metadata)
                {
                    return await ExchangeContract.Withdraw(profile,
                                                            item.Metadata.Get<string>("OwnerAddress"),
                                                            item.Metadata.Get<BigInteger>("Balance"));
                }
                else if (item.Metadata is ERC721GameItemContract.Metadata)
                {
                    return await ExchangeContract.WithdrawERC721(profile,
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
                throw new Exception(ex.ToString(), ex);
            }
        }

        /// <summary>
        /// Cancels existing order
        /// </summary>
        /// <param name="profile">creator of order</param>
        /// <param name="order">order to cancel</param>
        /// <returns>true if operation succeeded, flase otherwise</returns>
        public async Task<bool> CancelOrder(Profile profile, Order order)
        {
            if (order.gameItemGive.Metadata is ERC223GameItemContract.Metadata)
            {
                return await ExchangeContract.CancelOrder(
                   profile,
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
                    profile,
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
