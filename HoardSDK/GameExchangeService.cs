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

        public GameExchangeService(GBClient client, BC.BCComm bcComm)
        {
            this.client = client;
            this.bcComm = bcComm;
        }

        public async Task<Order[]> ListOrders(string tokenGet, string tokenGive)
        {
            var jsonStr = await client.GetJson(String.Format("exchange/orders/{0},{1}", tokenGet, tokenGive), null);

            var list = JsonConvert.DeserializeObject<Order[]>(jsonStr);

            return list;
        }
    }

    public class Order
    {
        [JsonProperty(propertyName: "tokenGet")]
        public string tokenGet { get; private set; }

        [JsonProperty(propertyName: "amountGet")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger amountGet { get; private set; }

        [JsonProperty(propertyName: "tokenGive")]
        public string tokenGive { get; private set; }

        [JsonProperty(propertyName: "amountGive")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger amountGive { get; private set; }

        [JsonProperty(propertyName: "expires")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger expires { get; private set; }

        [JsonProperty(propertyName: "nonce")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger nonce { get; private set; }

        [JsonProperty(propertyName: "amount")]
        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger amount { get; private set; }

        public Order(string tokenGet, BigInteger amountGet, string tokenGive, BigInteger amountGive, BigInteger expires, BigInteger nonce, BigInteger amount)
        {
            this.tokenGet = tokenGet;
            this.amountGet = amountGet;
            this.tokenGive = tokenGive;
            this.amountGive = amountGive;
            this.expires = expires;
            this.nonce = nonce;
            this.amount = amount;
        }
    }

    class BigIntegerConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(BigInteger));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Integer || 
                token.Type == JTokenType.String)
            {
                return new BigInteger(token.ToString());
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

}
