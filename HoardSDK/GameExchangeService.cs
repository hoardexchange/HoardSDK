using Hoard.BC.Contracts;
using Hoard.GameItemProviders;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard
{
    public interface IExchangeService
    {
        Task<bool> Deposit(GameItem item, ulong amount);

        Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive);

        Task<bool> Trade(Order order, ulong amount);

        Task<bool> Withdraw(GameItem item, ulong amount);
    }

    public class GameExchangeService : IExchangeService
    {
        private GameID Game = null;
        private BC.BCComm BCComm = null;
        private BC.Contracts.GameExchangeContract GameExchangeContract = null;
        private PlayerID PlayerID = null;
        private IGameItemProvider ItemProvider = null;

        private RestClient Client = null;

        public GameExchangeService(HoardService hoard, IGameItemProvider itemProvider)
        {
            this.BCComm = hoard.BCComm;
            this.PlayerID = hoard.DefaultPlayer;
            this.ItemProvider = itemProvider;
        }

        // Setup exchange backend client. 
        // Note: Lets assume it connects on its own, independently from item providers.
        private bool SetupClient(PlayerID player)
        {
            if (Uri.IsWellFormedUriString(Game.Url, UriKind.Absolute))
            {
                Client = new RestClient(Game.Url);
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

        public bool Init(GameID game)
        {
            Game = game;
            GameExchangeContract = BCComm.GetGameExchangeContract(game).Result;
            if (GameExchangeContract == null)
                return false;
            return SetupClient(PlayerID);
        }

        public void Shutdown()
        {
            GameExchangeContract = null;
            Client = null;
        }

        public async Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive)
        {
            var jsonStr = await GetJson(
                String.Format("exchange/orders/{0},{1}",
                gaGet != null ? gaGet.Metadata.Get<string>("OwnerAddress") : "",
                gaGive != null ? gaGive.Metadata.Get<string>("OwnerAddress") : ""), null);

            if (jsonStr != null)
            {
                var list = JsonConvert.DeserializeObject<Order[]>(jsonStr);

                foreach (var l in list)
                {
                    l.UpdateGameItemObjs(
                        CreateGameItem(l.tokenGet, 0), 
                        CreateGameItem(l.tokenGive, l.tokenId)
                    );
                }
                return list.ToList().FindAll(e => e.amount < e.amountGet).ToArray();
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

        public async Task<bool> Deposit(GameItem item, ulong amount)
        {
            return await ItemProvider.Transfer(PlayerID.ID, GameExchangeContract.Address, item, amount);
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

        private GameItem CreateGameItem(string itemContractAddress, BigInteger tokenId)
        {
            GameItemsParams gameItemsParams = new GameItemsParams();
            gameItemsParams.PlayerID = PlayerID;
            gameItemsParams.ContractAddress = itemContractAddress;
            gameItemsParams.TokenId = tokenId;

            return ItemProvider.GetItems(new GameItemsParams[] { gameItemsParams })[0];
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
