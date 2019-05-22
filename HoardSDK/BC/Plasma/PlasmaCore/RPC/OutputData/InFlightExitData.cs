using Newtonsoft.Json;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for in-flight exit data request
    /// </summary>
    public class InFlightExitData
    {
        [JsonProperty(propertyName: "in_flight_tx")]
        public string Tx { get; private set; }

        [JsonProperty(propertyName: "input_txs")]
        public string InputTxs { get; private set; }

        [JsonProperty(propertyName: "input_txs_inclusion_proofs")]
        public string InputTxsInclusionProofs { get; private set; }

        [JsonProperty(propertyName: "in_flight_tx_sigs")]
        public string TxSigs { get; private set; }
    }
}
