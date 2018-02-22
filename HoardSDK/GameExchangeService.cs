using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Math;
using Newtonsoft.Json.Linq;

namespace Hoard
{
    public class GameExchangeService
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

        public async Task<Order[]> ListOrders(GameAsset gaGet, GameAsset gaGive)
        {
            var jsonStr = await client.GetJson(
                String.Format("exchange/orders/{0},{1}",
                gaGet != null ? gaGet.ContractAddress : "",
                gaGive != null ? gaGive.ContractAddress : ""), null);

            var list = JsonConvert.DeserializeObject<Order[]>(jsonStr);

            foreach (var l in list)
            {
                l.UpdateGameAssetsObjs(
                    this.hoard.GameAssetAddressDict[l.tokenGet.ToLower()],
                    this.hoard.GameAssetAddressDict[l.tokenGive.ToLower()]
                    );
            }
                

            return list.ToList().FindAll(e => e.amount < e.amountGet).ToArray();
        }

        public async Task<bool> Trade(Order order, ulong amount)
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
                hoard.Accounts[0].Address);
        }

        public async Task<bool> Deposit(GameAsset asset, ulong amount)
        {
            return await asset.Contract.Transfer(GameExchangeContract.Address, amount, hoard.Accounts[0].Address);
        }

        public async Task<bool> Withdraw(GameAsset asset, ulong amount)
        {
            return await GameExchangeContract.Withdraw(asset.ContractAddress, amount, hoard.Accounts[0].Address);
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

        public GameAsset gameAssetGet { get; private set; } = null;
        public GameAsset gameAssetGive { get; private set; } = null;

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

        public void UpdateGameAssetsObjs(GameAsset gaGet, GameAsset gaGive)
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
