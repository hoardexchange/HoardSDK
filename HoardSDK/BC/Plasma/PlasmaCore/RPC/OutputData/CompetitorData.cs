using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    public class CompetitorData
    {
        [JsonProperty(propertyName: "in_flight_txbytes")]
        public string TxBytes { get; private set; }

        [JsonProperty(propertyName: "in_flight_input_index")]
        public UInt16 InputIndex { get; private set; }

        [JsonProperty(propertyName: "competing_txbytes")]
        public string CompetingTxBytes { get; private set; }

        [JsonProperty(propertyName: "competing_input_index")]
        public UInt16 CompetingInputIndex { get; private set; }

        [JsonProperty(propertyName: "competing_sig")]
        public string CompetingSignature { get; private set; }

        [JsonProperty(propertyName: "competing_tx_pos")]
        public BigInteger CompetingTxPos { get; private set; }

        [JsonProperty(propertyName: "competing_proof")]
        public string CompetingProof { get; private set; }
    }
}
