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

        [JsonProperty(propertyName: "input_index")]
        public UInt16 InputIndex { get; private set; }

        // used only in v0.1 (Ari)
        [JsonProperty(propertyName: "utxo_pos")]
        public BigInteger Position { get; private set; }

        [JsonProperty(propertyName: "sig")]
        public string Signature { get; private set; }

        [JsonProperty(propertyName: "txbytes")]
        public string TxBytes { get; private set; }
    }
}
