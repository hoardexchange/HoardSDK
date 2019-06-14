using Newtonsoft.Json;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for in-flight exit data request
    /// </summary>
    public class InFlightExitData
    {
        /// <summary>
        /// Exit transaction data
        /// </summary>
        [JsonProperty(propertyName: "in_flight_tx")]
        public string Tx { get; private set; }

        /// <summary>
        /// Input transactions
        /// </summary>
        [JsonProperty(propertyName: "input_txs")]
        public string InputTxs { get; private set; }

        /// <summary>
        /// Transactions inclusion proofs
        /// </summary>
        [JsonProperty(propertyName: "input_txs_inclusion_proofs")]
        public string InputTxsInclusionProofs { get; private set; }

        /// <summary>
        /// Signatures for all transactions
        /// </summary>
        [JsonProperty(propertyName: "in_flight_tx_sigs")]
        public string TxSigs { get; private set; }
    }
}
