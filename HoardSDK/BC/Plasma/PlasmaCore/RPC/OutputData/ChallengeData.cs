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
