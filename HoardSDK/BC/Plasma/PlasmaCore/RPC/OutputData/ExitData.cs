using Newtonsoft.Json;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for exit data request
    /// </summary>
    [JsonObject]
    public class ExitData
    {
        /// <summary>
        /// UTXO position
        /// </summary>
        [JsonProperty(propertyName: "utxo_pos")]
        public BigInteger Position { get; set; }

        /// <summary>
        /// Transaction byte stream
        /// </summary>
        [JsonProperty(propertyName: "txbytes")]
        public string TxBytes { get; set; }

        /// <summary>
        /// Exit data proof
        /// </summary>
        [JsonProperty(propertyName: "proof")]
        public string Proof { get; set; }

        /// <summary>
        /// Timestamp when exit can be processed
        /// </summary>
        [JsonProperty(propertyName: "process_timestamp")]
        public BigInteger ProcessTimestamp { get; set; }

        [JsonConstructor]
        private ExitData() { }
    }
}
