using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for canonical proof request
    /// </summary>
    public class CanonicalProofData
    {
        /// <summary>
        /// transaction byte stream
        /// </summary>
        [JsonProperty(propertyName: "in_flight_txbytes")]
        public string TxBytes { get; private set; }

        /// <summary>
        /// transaction position in chain data
        /// </summary>
        [JsonProperty(propertyName: "in_flight_tx_pos")]
        public BigInteger TxPosition { get; private set; }

        /// <summary>
        /// transaction proof
        /// </summary>
        [JsonProperty(propertyName: "in_flight_proof")]
        public string TxProof { get; private set; }
    }
}
