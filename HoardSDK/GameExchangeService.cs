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

        Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive);

        Task<bool> Order(GameItem item, ulong amount);

        Task<bool> Trade(Order order, ulong amount);

        Task<bool> Withdraw(GameItem item, ulong amount);
    }

    public class GameExchangeService : IExchangeService
    {
        HoardService Hoard = null;
        private string ExchangUrl = null;
        private BC.BCComm BCComm = null;
        private BC.Contracts.GameExchangeContract GameExchangeContract = null;
        private PlayerID PlayerID = null;

        private RestClient Client = null;

        public GameExchangeService(HoardService hoard)
        {
            this.Hoard = hoard;
            this.BCComm = hoard.BCComm;
            this.PlayerID = hoard.DefaultPlayer;
        }

        public bool Init()
        {
            GameExchangeContract = BCComm.GetGameExchangeContract().Result;
            if (GameExchangeContract == null)
                return false;
            ExchangUrl = BCComm.GetGameExchangeSrvURL().Result;
            return SetupClient(PlayerID);
        }

        public void Shutdown()
        {
            GameExchangeContract = null;
            Client = null;
        }

        // Setup exchange backend client. 
        // Note: Lets assume it connects on its own, independently from item providers.
        private bool SetupClient(PlayerID player)
        {
            if (Uri.IsWellFormedUriString(ExchangUrl, UriKind.Absolute))
            {
                Client = new RestClient(ExchangUrl);
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

        public async Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive)
        {
            var jsonStr = await GetJson(
                String.Format("exchange/orders/{0},{1}",
                gaGet != null ? gaGet.Metadata.Get<string>("OwnerAddress") : "",
                gaGive != null ? gaGive.Metadata.Get<string>("OwnerAddress") : ""), null);

            HashSet<GameItemId> itemsIds = new HashSet<GameItemId>();

            if (jsonStr != null)
            {
                var list = JsonConvert.DeserializeObject<Order[]>(jsonStr);

                var filteredList = list.ToList().FindAll(e => e.amount < e.amountGet).ToArray();

                foreach (var l in filteredList)
                {
                    itemsIds.Add(new GameItemId(l.tokenGet, "0"));
                    itemsIds.Add(new GameItemId(l.tokenGive, l.tokenId.ToString()));
                }

                GameItemsParams[] gameItemsParams = new GameItemsParams[itemsIds.Count];
                int itemsCount = 0;
                foreach (var item in itemsIds)
                {
                    gameItemsParams[itemsCount] = new GameItemsParams();
                    gameItemsParams[itemsCount].PlayerAddress = PlayerID.ID;
                    gameItemsParams[itemsCount].ContractAddress = item.Address;
                    gameItemsParams[itemsCount].TokenId = item.TokenId;
                    itemsCount++;
                }
                GameItem[] itemsRetrieved = Hoard.GetItems(gameItemsParams);

                foreach (var l in filteredList)
                {
                    l.UpdateGameItemObjs(
                        SearchGameItem(itemsRetrieved, l.tokenGet, 0),
                        SearchGameItem(itemsRetrieved, l.tokenGive, l.tokenId)
                    );
                }

                return filteredList;
            }

            return new Order[0];
        }

        public async Task<bool> Trade(Order order, ulong amount)
        {
            if (order.gameItemGive.Metadata is ERC223GameItemContract.Metadata)
            {
                return await GameExchangeContract.Trade(
                    order.tokenGet,
                    order.amountGet,
                    order.tokenGive,
                    order.amountGive,
                    order.expires,
                    order.nonce,
                    order.user,
                    amount,
                    PlayerID.ID);
            }
            else if (order.gameItemGive.Metadata is ERC721GameItemContract.Metadata)
            {
                return await GameExchangeContract.TradeERC721(
                    order.tokenGet,
                    order.amountGet,
                    order.tokenGive,
                    order.tokenId,
                    order.expires,
                    order.nonce,
                    order.user,
                    amount,
                    PlayerID.ID);
            }

            throw new NotImplementedException();
        }

        public async Task<bool> Order(GameItem item, ulong amount)
        {
            if (item.Metadata is ERC721GameItemContract.Metadata)
            {
                return await GameExchangeContract.OrderERC721(
                    Hoard.GetHRDAddress(),
                    1,
                    item.Metadata.Get<string>("OwnerAddress"),
                    item.Metadata.Get<BigInteger>("ItemId"),
                    PlayerID);
            }

            throw new NotImplementedException();
        }

        public async Task<bool> Deposit(GameItem item, ulong amount)
        {
            IGameItemProvider gameItemProvider = Hoard.GetGameItemProvider(item);
            if (gameItemProvider != null)
            {
                return await gameItemProvider.Transfer(PlayerID.ID, GameExchangeContract.Address, item, amount);
            }
            return false;
        }

        public async Task<bool> Withdraw(GameItem item, ulong amount)
        {
            var metadata223 = item.Metadata as ERC223GameItemContract.Metadata;
            if (metadata223 != null)
            {
                return await GameExchangeContract.Withdraw(metadata223.OwnerAddress, amount, PlayerID.ID);
            }

            var metadata721 = item.Metadata as ERC721GameItemContract.Metadata;
            if (metadata721 != null)
            {
                return await GameExchangeContract.WithdrawERC721(metadata721.OwnerAddress, metadata721.ItemId, PlayerID.ID);
            }

            throw new NotImplementedException();
        }

        private GameItem SearchGameItem(IEnumerable items, string itemContractAddress, BigInteger tokenId)
        {
            foreach (GameItem item in items)
            {
                if (item.Metadata.Get<string>("OwnerAddress") == itemContractAddress.ToLower())
                {
                    var metadata721 = item.Metadata as ERC721GameItemContract.Metadata;
                    if (metadata721 != null)
                    {
                        if (metadata721.ItemId == tokenId)
                            return item;
                    }
                    return item;
                }
            }
            return null;
        }
    }

    public class Order
    {
        [JsonProperty(propertyName: "tokenGet")]
        public string tokenGet { get; private set; }

        [JsonProperty(propertyName: "amountGet")]
        public ulong amountGet { get; private set; }

        [JsonProperty(propertyName: "tokenGive")]
        public string tokenGive { get; private set; }

        [JsonProperty(propertyName: "tokenId")]
        public BigInteger tokenId { get; private set; }

        [JsonProperty(propertyName: "amountGive")]
        public ulong amountGive { get; private set; }

        [JsonProperty(propertyName: "expires")]
        public ulong expires { get; private set; }

        [JsonProperty(propertyName: "nonce")]
        public ulong nonce { get; private set; }

        [JsonProperty(propertyName: "amount")]
        public ulong amount { get; private set; }

        [JsonProperty(propertyName: "user")]
        public string user { get; private set; }

        public GameItem gameItemGet { get; private set; } = null;
        public GameItem gameItemGive { get; private set; } = null;

        public Order(string tokenGet, ulong amountGet, string tokenGive, ulong amountGive, ulong expires, ulong nonce, ulong amount, string user)
        {
            this.tokenGet = tokenGet;
            this.amountGet = amountGet;
            this.tokenGive = tokenGive;
            this.amountGive = amountGive;
            this.expires = expires;
            this.nonce = nonce;
            this.amount = amount;
            this.user = user;
        }

        public void UpdateGameItemObjs(GameItem gaGet, GameItem gaGive)
        {
            gameItemGet = gaGet;
            gameItemGive = gaGive;
        }
    }
}
