using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for challenge request
    /// </summary>
    public class ChallengeData
    {
        // used only in v0.2 (Samrong)
        [JsonProperty(propertyName: "exit_id")]
        public BigInteger ExitId { get; private set; }

        /// <summary>
        /// Input index
        /// </summary>
        [JsonProperty(propertyName: "input_index")]
        public ushort InputIndex { get; private set; }

        /// <summary>
        /// UTXO position in chain
        /// </summary>
        [JsonProperty(propertyName: "utxo_pos")]
        public BigInteger Position { get; private set; }

        /// <summary>
        /// Challenge signature
        /// </summary>
        [JsonProperty(propertyName: "sig")]
        public string Signature { get; private set; }

        /// <summary>
        /// Transaction byte stream
        /// </summary>
        [JsonProperty(propertyName: "txbytes")]
        public string TxBytes { get; private set; }
    }
}
