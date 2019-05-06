using Newtonsoft.Json;
using System.Numerics;

namespace Hoard.BC.Plasma
{
    /// <summary>
    /// Description of token data
    /// </summary>
    public class TokenData
    {
        /// <summary>
        /// Token currency
        /// </summary>
        [JsonProperty(propertyName: "currency")]
        public string Currency { get; private set; }

        /// <summary>
        /// Token amount (only applicable to ERC223)
        /// </summary>
        [JsonProperty(propertyName: "amount")]
        public BigInteger Amount { get; private set; }

        /// <summary>
        /// Token id (only applicable to ERC721)
        /// </summary>
        [JsonProperty(propertyName: "tokenId")]
        public BigInteger TokenId { get; private set; }
    }
}
