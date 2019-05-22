using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    public class CanonicalProofData
    {
        [JsonProperty(propertyName: "in_flight_txbytes")]
        public string TxBytes { get; private set; }

        [JsonProperty(propertyName: "in_flight_tx_pos")]
        public BigInteger TxPosition { get; private set; }

        [JsonProperty(propertyName: "in_flight_proof")]
        public string TxProof { get; private set; }
    }
}
