using Newtonsoft.Json;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    public class ExitData
    {
        [JsonProperty(propertyName: "utxo_pos")]
        public BigInteger Position { get; private set; }

        [JsonProperty(propertyName: "txbytes")]
        public string TxBytes { get; private set; }

        [JsonProperty(propertyName: "proof")]
        public string Proof { get; private set; }
    }
}
