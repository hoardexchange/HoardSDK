using Newtonsoft.Json;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Description of token data
    /// </summary>
    public class FCBalanceData : BalanceData
    {
        /// <summary>
        /// Currency amount
        /// </summary>
        [JsonProperty(propertyName: "amount")]
        public BigInteger Amount { get; private set; }
    }
}
