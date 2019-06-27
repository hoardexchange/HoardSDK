using Newtonsoft.Json;
using PlasmaCore.RPC.OutputData.Balance;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for account balance request
    /// </summary>
    [JsonObject]
    [JsonConverter(typeof(BalanceConverter))]
    public class BalanceData
    {        
        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty(propertyName: "currency")]
        public string Currency { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        [JsonConstructor]
        protected BalanceData() { }
    }
}
