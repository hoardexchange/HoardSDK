using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.UTXO
{
    /// <summary>
    /// Response class for unspent transaction output request
    /// </summary>
    [JsonConverter(typeof(UTXOConverter))]
    public class UTXOData
    {
        /// <summary>
        /// Transaction index within the block
        /// </summary>
        [JsonProperty(propertyName: "txindex")]
        public UInt16 TxIndex { get; set; }

        /// <summary>
        /// Transaction output index
        /// </summary>
        [JsonProperty(propertyName: "oindex")]
        public UInt16 OIndex { get; set; }

        /// <summary>
        /// Currency of the transaction (all zeroes for ETH)
        /// </summary>
        [JsonProperty(propertyName: "currency")]
        public string Currency { get; protected set; }

        /// <summary>
        /// Block number
        /// </summary>
        [JsonProperty(propertyName: "blknum")]
        public UInt64 BlkNum { get; set; }

        /// <summary>
        /// UTXO position
        /// </summary>
        [JsonProperty(propertyName: "utxo_pos")]
        public BigInteger Position { get; protected set; }

        /// <summary>
        /// Owner of utxo
        /// </summary>
        [JsonProperty(propertyName: "owner")]
        public string Owner { get; protected set; }
    }
}
