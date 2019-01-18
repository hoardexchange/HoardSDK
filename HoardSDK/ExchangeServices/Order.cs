using Hoard;
using Newtonsoft.Json;
using System.Numerics;

namespace Hoard.ExchangeServices
{
    /// <summary>
    /// Description of Trade Order
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Currency of order
        /// </summary>
        [JsonProperty(propertyName: "tokenGet")]
        public string tokenGet { get; private set; }

        /// <summary>
        /// Price of order in currency
        /// </summary>
        [JsonProperty(propertyName: "amountGet")]
        public BigInteger amountGet { get; private set; }

        /// <summary>
        /// Item to trade
        /// </summary>
        [JsonProperty(propertyName: "tokenGive")]
        public string tokenGive { get; private set; }

        /// <summary>
        /// Identifier of item to trade (in case of ERC721)
        /// </summary>
        [JsonProperty(propertyName: "tokenId")]
        public BigInteger tokenIdGive { get; private set; }

        /// <summary>
        /// Amount of items to trade (in case of ERC223)
        /// </summary>
        [JsonProperty(propertyName: "amountGive")]
        public BigInteger amountGive { get; private set; }

        /// <summary>
        /// Expiration time of oreder (in number of blocks)
        /// </summary>
        [JsonProperty(propertyName: "expires")]
        public BigInteger expires { get; private set; }

        /// <summary>
        /// Transaction identifier (useful when there are several exactly same orders in network)
        /// </summary>
        [JsonProperty(propertyName: "nonce")]
        public BigInteger nonce { get; private set; }

        /// <summary>
        /// Amount of items to buy (in case of ERC223)
        /// </summary>
        [JsonProperty(propertyName: "amount")]
        public BigInteger amount { get; set; }

        /// <summary>
        /// Account address of order dispatcher (who is going to consume order)
        /// </summary>
        [JsonProperty(propertyName: "user")]
        public string user { get; private set; }

        /// <summary>
        /// Price in GameItem
        /// </summary>
        public GameItem gameItemGet { get; private set; } = null;
        /// <summary>
        /// GameItem to trade
        /// </summary>
        public GameItem gameItemGive { get; private set; } = null;

        /// <summary>
        /// Sets new price and trade item
        /// </summary>
        /// <param name="gaGet">Price</param>
        /// <param name="gaGive">Item to trade</param>
        public void UpdateGameItemObjs(GameItem gaGet, GameItem gaGive)
        {
            gameItemGet = gaGet;
            gameItemGive = gaGive;
        }
    }
}
