using Newtonsoft.Json;
using PlasmaCore.RPC.OutputData.Balance;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Description of currency data
    /// </summary>
    [JsonConverter(typeof(BalanceConverter))]
    public class BalanceData
    {
        /// <summary>
        /// Token currency
        /// </summary>
        [JsonProperty(propertyName: "currency")]
        public string Currency { get; private set; }
    }
}
