using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for competitor request
    /// </summary>
    public class CompetitorData
    {
        /// <summary>
        /// Transaction data in bytes to be challenged
        /// </summary>
        [JsonProperty(propertyName: "in_flight_txbytes")]
        public string TxBytes { get; private set; }

        /// <summary>
        /// Input index
        /// </summary>
        [JsonProperty(propertyName: "in_flight_input_index")]
        public UInt16 InputIndex { get; private set; }

        /// <summary>
        /// Competing transaction data in bytes
        /// </summary>
        [JsonProperty(propertyName: "competing_txbytes")]
        public string CompetingTxBytes { get; private set; }

        /// <summary>
        /// Input index of competing transaction
        /// </summary>
        [JsonProperty(propertyName: "competing_input_index")]
        public UInt16 CompetingInputIndex { get; private set; }

        /// <summary>
        /// Signature of competing transaction
        /// </summary>
        [JsonProperty(propertyName: "competing_sig")]
        public string CompetingSignature { get; private set; }

        /// <summary>
        /// Position of competeing transaction
        /// </summary>
        [JsonProperty(propertyName: "competing_tx_pos")]
        public BigInteger CompetingTxPos { get; private set; }

        /// <summary>
        /// Proof of competing transaction
        /// </summary>
        [JsonProperty(propertyName: "competing_proof")]
        public string CompetingProof { get; private set; }
    }
}
