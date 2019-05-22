using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    public class ChallengeData
    {
        [JsonProperty(propertyName: "input_index")]
        public UInt16 InputIndex { get; private set; }

        [JsonProperty(propertyName: "utxo_pos")]
        public BigInteger Position { get; private set; }

        [JsonProperty(propertyName: "sig")]
        public string Signature { get; private set; }

        [JsonProperty(propertyName: "txbytes")]
        public string TxBytes { get; private set; }
    }
}
