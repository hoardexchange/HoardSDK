using Newtonsoft.Json;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for exit data request
    /// </summary>
    public class ExitData
    {
        /// <summary>
        /// UTXO position
        /// </summary>
        [JsonProperty(propertyName: "utxo_pos")]
        public BigInteger Position { get; private set; }

        /// <summary>
        /// Transaction byte stream
        /// </summary>
        [JsonProperty(propertyName: "txbytes")]
        public string TxBytes { get; private set; }

        /// <summary>
        /// Exit data proof
        /// </summary>
        [JsonProperty(propertyName: "proof")]
        public string Proof { get; private set; }
    }
}
