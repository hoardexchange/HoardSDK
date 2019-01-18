using Newtonsoft.Json;
using System.Numerics;

namespace Hoard.BC.Plasma
{
    public class BalanceData
    {
        [JsonProperty(propertyName: "currency")]
        public string Currency { get; private set; }

        [JsonProperty(propertyName: "amount")]
        public BigInteger Amount { get; private set; }
    }
}
