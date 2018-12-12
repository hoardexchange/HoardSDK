using Hoard.BC.Contracts;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard
{
    public interface IExchangeService
    {
        Task<bool> Deposit(GameItem item, ulong amount);

        Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive, AccountInfo account);

        Task<bool> Order(GameItem getItem, GameItem giveItem, ulong blockTimeDuration);

        Task<bool> Trade(Order order);

        Task<bool> Withdraw(GameItem item);
    }

    public class HoardExchangeService : IExchangeService
    {
        HoardService Hoard = null;
        private BC.BCComm BCComm = null;
        private ExchangeContract ExchangeContract = null;
        private User User = null;

        private RestClient Client = null;

        public HoardExchangeService(HoardService hoard)
        {
            this.Hoard = hoard;
            this.BCComm = hoard.BCComm;
            this.User = hoard.DefaultUser;
        }

        public bool Init()
        {
            ExchangeContract = BCComm.GetGameExchangeContractAsync().Result;
            if (ExchangeContract == null)
                return false;
            return SetupClient(User);
        }

        public void Shutdown()
        {
            ExchangeContract = null;
            Client = null;
        }

        // Setup exchange backend client. 
        // Note: Lets assume it connects on its own, independently from item providers.
        private bool SetupClient(User user)
        {
            if (Uri.IsWellFormedUriString(Hoard.Options.ExchangeServiceUrl, UriKind.Absolute))
            {
                Client = new RestClient(Hoard.Options.ExchangeServiceUrl);
                Client.AutomaticDecompression = false;

                //setup a cookie container for automatic cookies handling
                Client.CookieContainer = new System.Net.CookieContainer();

                return true;
            }
            return false;
        }

        private async Task<string> GetJson(string url, object data)
        {
            var request = new RestRequest(url, Method.GET);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            request.AddJsonBody(data);

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response.Content;
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
            var jsonStr = await GetJson(
                String.Format("exchange/orders/{0},{1},{2}",
                gaGet != null ? gaGet.Metadata.Get<string>("OwnerAddress") : "",
                gaGive != null ? gaGive.Metadata.Get<string>("OwnerAddress") : "",
                account != null ? account.ID : ""), null);

            if (jsonStr != null)
            {
                var orders = JsonConvert.DeserializeObject<Order[]>(jsonStr);
                GameItemsParams[] gameItemsParams = new GameItemsParams[orders.Length * 2];
                for (var i = 0; i < orders.Length; ++i)
                {
                    gameItemsParams[i * 2] = new GameItemsParams();
                    gameItemsParams[i * 2].ContractAddress = orders[i].tokenGive;
                    gameItemsParams[i * 2].TokenId = orders[i].tokenIdGive.ToString();

                    gameItemsParams[i * 2 + 1] = new GameItemsParams();
                    gameItemsParams[i * 2 + 1].ContractAddress = orders[i].tokenGet;
                }

                GameItem[] itemsRetrieved = Hoard.GetItems(gameItemsParams);
                for (var i = 0; i < orders.Length; ++i)
                {
                    orders[i].UpdateGameItemObjs(itemsRetrieved[i * 2 + 1], itemsRetrieved[i * 2]);
                }

                return orders;
            }

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

        public void SetUser(User user)
        {
            User = user;
        }
    }

    public class Order
    {
        [JsonProperty(propertyName: "tokenGet")]
        public string tokenGet { get; private set; }

        [JsonProperty(propertyName: "amountGet")]
        public BigInteger amountGet { get; private set; }

        [JsonProperty(propertyName: "tokenGive")]
        public string tokenGive { get; private set; }

        [JsonProperty(propertyName: "tokenId")]
        public BigInteger tokenIdGive { get; private set; }

        [JsonProperty(propertyName: "amountGive")]
        public BigInteger amountGive { get; private set; }

        [JsonProperty(propertyName: "expires")]
        public BigInteger expires { get; private set; }

        [JsonProperty(propertyName: "nonce")]
        public BigInteger nonce { get; private set; }

        [JsonProperty(propertyName: "amount")]
        public BigInteger amount { get; set; }

        [JsonProperty(propertyName: "user")]
        public string user { get; private set; }

        public GameItem gameItemGet { get; private set; } = null;
        public GameItem gameItemGive { get; private set; } = null;

        public void UpdateGameItemObjs(GameItem gaGet, GameItem gaGive)
        {
            gameItemGet = gaGet;
            gameItemGive = gaGive;
        }
    }
}
