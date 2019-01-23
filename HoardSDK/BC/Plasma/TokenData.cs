using Newtonsoft.Json;
using System.Numerics;

namespace Hoard.BC.Plasma
{
    public class TokenData
    {
        [JsonProperty(propertyName: "currency")]
        public string Currency { get; private set; }

        [JsonProperty(propertyName: "amount")]
        public BigInteger Amount { get; private set; }

        [JsonProperty(propertyName: "tokenId")]
        public BigInteger TokenId { get; private set; }
    }
}
