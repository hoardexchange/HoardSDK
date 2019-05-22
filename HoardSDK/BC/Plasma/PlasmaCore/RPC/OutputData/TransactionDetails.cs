using Newtonsoft.Json;
using PlasmaCore.UTXO;
using System;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for transaction details request
    /// </summary>
    public class TransactionDetails
    {
        /// <summary>
        /// Transaction index
        /// </summary>
        [JsonProperty(propertyName: "txindex")]
        public UInt16 TxIndex { get; private set; }

        /// <summary>
        /// Transaction hash
        /// </summary>
        [JsonProperty(propertyName: "txhash")]
        public string TxHash { get; private set; }

        /// <summary>
        /// Transaction bytes
        /// </summary>
        [JsonProperty(propertyName: "txbytes")]
        public string TxBytes { get; private set; }

        /// <summary>
        /// Transaction block
        /// </summary>
        [JsonProperty(propertyName: "block")]
        public BlockData Block { get; private set; }

        /// <summary>
        /// Transaction utxo inputs
        /// </summary>
        [JsonProperty(propertyName: "inputs")]
        public UTXOData[] Inputs { get; private set; }

        /// <summary>
        /// Transaction utxo outputs
        /// </summary>
        [JsonProperty(propertyName: "outputs")]
        public UTXOData[] Outputs { get; private set; }
    }
}
