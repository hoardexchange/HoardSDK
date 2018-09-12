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
    public class ExchangeService
    {
        public virtual Task<bool> Deposit(GameItem item, ulong amount)
        {
            throw new NotImplementedException();
        }

        public virtual Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> Trade(Order order, ulong amount)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> Withdraw(GameItem item, ulong amount)
        {
            throw new NotImplementedException();
        }
    }

    public class GameExchangeService : ExchangeService
    {
        private GameID Game = null;
        private BC.BCComm BCComm = null;
        private BC.Contracts.GameExchangeContract GameExchangeContract = null;
        private PlayerID PlayerID = null;
        private IGameItemProvider ItemProvider = null;

        private RestClient Client = null;
        private string SessionKey = null;

        public GameExchangeService(HoardService hoard, IGameItemProvider itemProvider)
        {
            this.BCComm = hoard.BCComm;
            this.PlayerID = hoard.DefaultPlayer;
            this.ItemProvider = itemProvider;
        }

        #region Backend connection

        // Connect to exchange backend. 
        // Note: Lets assume it connects on its own, independently from item providers.
        private bool Connect(PlayerID player)
        {
            if (Uri.IsWellFormedUriString(Game.Url, UriKind.Absolute))
            {
                Client = new RestClient(Game.Url);
                Client.AutomaticDecompression = false;

                //setup a cookie container for automatic cookies handling
                Client.CookieContainer = new System.Net.CookieContainer();

                //handshake

                //1. GET challenge token
                var request = new RestRequest("login/", Method.GET);
                request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
                var response = Client.Execute(request);

                if (response.ErrorException != null)
                    return false;

                //UpdateCookies(response.Cookies);

                string challengeToken = response.Content;
                challengeToken = challengeToken.Substring(2);


                var nonce = Hoard.Eth.Utils.Mine(challengeToken, new BigInteger(1) << 496);
                var nonceHex = nonce.ToString("x");

                var sig = Hoard.Eth.Utils.Sign(response.Content.Substring(2) + nonceHex, player.PrivateKey);

                var responseLogin = PostJson("login/", new
                {
                    token = response.Content,
                    nonce = "0x" + nonceHex,
                    address = player.ID,
                    signature = sig
                }).Result;

                if (responseLogin.StatusCode != System.Net.HttpStatusCode.OK || responseLogin.Content != "Logged in")
                    return false;

                SessionKey = response.Content;

                return true;
            }

            return false;
        }

        private void PrepareRequest(RestRequest req)
        {
            var cookies = Client.CookieContainer.GetCookies(new Uri(Game.Url));
            req.AddHeader("X-CSRFToken", cookies["csrftoken"].Value);
        }

        private async Task<IRestResponse> PostJson(string url, object data)
        {
            var request = new RestRequest(url, Method.POST);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            request.AddJsonBody(data);

            PrepareRequest(request);

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response;
        }

        public async Task<string> GetJson(string url, object data)
        {
            var request = new RestRequest(url, Method.GET);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            request.AddJsonBody(data);
            PrepareRequest(request);

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response.Content;
        }

        #endregion

        public bool Init(GameID game)
        {
            Game = game;
            GameExchangeContract = BCComm.GetGameExchangeContract(game).Result;
            return Connect(PlayerID);
        }

        public void Shutdown()
        {
            GameExchangeContract = null;
            Client = null;
        }

        public override async Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive)
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

        public override async Task<bool> Trade(Order order, ulong amount)
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

        public override async Task<bool> Deposit(GameItem item, ulong amount)
        {
            return await ItemProvider.Transfer(PlayerID.ID, GameExchangeContract.Address, item, amount);
        }

        public override async Task<bool> Withdraw(GameItem item, ulong amount)
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
        // [JsonConverter(typeof(BigIntegerConverter))]
        public ulong amountGet { get; private set; }

        [JsonProperty(propertyName: "tokenGive")]
        public string tokenGive { get; private set; }

        [JsonProperty(propertyName: "tokenId")]
        // [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger tokenId { get; private set; }

        [JsonProperty(propertyName: "amountGive")]
        // [JsonConverter(typeof(BigIntegerConverter))]
        public ulong amountGive { get; private set; }

        [JsonProperty(propertyName: "expires")]
        // [JsonConverter(typeof(BigIntegerConverter))]
        public ulong expires { get; private set; }

        [JsonProperty(propertyName: "nonce")]
        // [JsonConverter(typeof(BigIntegerConverter))]
        public ulong nonce { get; private set; }

        [JsonProperty(propertyName: "amount")]
        // [JsonConverter(typeof(BigIntegerConverter))]
        public ulong amount { get; private set; }

        [JsonProperty(propertyName: "user")]
        // [JsonConverter(typeof(BigIntegerConverter))]
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

    //class BigIntegerConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        return (objectType == typeof(BigInteger));
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        JToken token = JToken.Load(reader);
    //        if (token.Type == JTokenType.Integer || 
    //            token.Type == JTokenType.String)
    //        {
    //            return new BigInteger(token.ToString());
    //        }

    //        return null;
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        serializer.Serialize(writer, value);
    //    }
    //}
}
