using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hoard
{
    public class ExchangeService
    {
        public virtual Task<bool> Deposit(GameItem asset, ulong amount)
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

        public virtual Task<bool> Withdraw(GameItem asset, ulong amount)
        {
            throw new NotImplementedException();
        }
    }

    public class GameExchangeService : ExchangeService
    {
        private BC.BCComm bcComm = null;
        private GBClient client = null;
        private BC.Contracts.GameExchangeContract GameExchangeContract = null;
        private HoardService hoard = null;

        public GameExchangeService(GBClient client, BC.BCComm bcComm, HoardService hoard)
        {
            this.client = client;
            this.bcComm = bcComm;
            this.hoard = hoard; // FIXME: Circular dep.
        }

        public void Init(BC.Contracts.GameContract gameContract)
        {
            GameExchangeContract = bcComm.GetContract<BC.Contracts.GameExchangeContract>(gameContract.GameExchangeContractAsync().Result);
        }

        public void Shutdown()
        {
            GameExchangeContract = null;
        }

        // FIXME
        public override async Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive)
        {
            var jsonStr = await client.GetJson(
                String.Format("exchange/orders/{0},{1}",
                /*gaGet != null ? gaGet.ContractAddress :*/ "",
                /*gaGive != null ? gaGive.ContractAddress :*/ ""), null);

            if (jsonStr != null)
            {
                var list = JsonConvert.DeserializeObject<Order[]>(jsonStr);

                foreach (var l in list)
                {
                    //l.UpdateGameAssetsObjs(
                    //    this.hoard.GameAssetAddressDict[l.tokenGet.ToLower()],
                    //    this.hoard.GameAssetAddressDict[l.tokenGive.ToLower()]
                    //    );
                }
                return list.ToList().FindAll(e => e.amount < e.amountGet).ToArray();
            }

            return new Order[0];
        }

        public override async Task<bool> Trade(Order order, ulong amount)
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
                hoard.DefaultPlayer.ID);
        }

        public override async Task<bool> Deposit(GameItem asset, ulong amount)
        {
            //return await asset.Contract.Transfer(GameExchangeContract.Address, amount, hoard.Accounts[0].Address);
            throw new NotImplementedException();
        }

        public override async Task<bool> Withdraw(GameItem asset, ulong amount)
        {
            //return await GameExchangeContract.Withdraw(asset.ContractAddress, amount, hoard.Accounts[0].Address);
            throw new NotImplementedException();
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

        public GameItem gameAssetGet { get; private set; } = null;
        public GameItem gameAssetGive { get; private set; } = null;

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

        public void UpdateGameAssetsObjs(GameItem gaGet, GameItem gaGive)
        {
            gameAssetGet = gaGet;
            gameAssetGive = gaGive;
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
