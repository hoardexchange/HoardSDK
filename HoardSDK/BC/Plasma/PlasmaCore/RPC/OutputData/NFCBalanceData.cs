using Newtonsoft.Json;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for account balance request (non fungible currencies)
    /// </summary>
    public class NFCBalanceData : BalanceData
    {
        /// <summary>
        /// Tokend identifiers
        /// </summary>
        [JsonProperty(propertyName: "tokenids")]
        public BigInteger[] TokenIds { get; private set; }
    }
}
