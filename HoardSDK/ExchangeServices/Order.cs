using Hoard;
using Newtonsoft.Json;
using System.Numerics;

namespace HoardSDK.ExchangeServices
{
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
