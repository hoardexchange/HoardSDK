using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.UTXO
{
    /// <summary>
    /// Response class for unspent transaction output request
    /// </summary>
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
        public string Currency { get; set; }

        /// <summary>
        /// Block number
        /// </summary>
        [JsonProperty(propertyName: "blknum")]
        public ulong BlkNum { get; set; }

        /// <summary>
        /// UTXO position
        /// </summary>
        [JsonProperty(propertyName: "utxo_pos")]
        public BigInteger Position { get; set; }

        /// <summary>
        /// Owner of utxo
        /// </summary>
        [JsonProperty(propertyName: "owner")]
        public string Owner { get; set; }

        /// <summary>
        /// Utxo data - amount of tokens (fungible currencies) / token identifier (non fungible currencies)
        /// </summary>
        [JsonProperty(propertyName: "amount")]
        public BigInteger Data { get; set; }

        /// <summary>
        /// Returns position based on block number, transaction index and output index
        /// </summary>
        /// <param name="blkNum">block number</param>
        /// <param name="txIndex">transaction index</param>
        /// <param name="oIndex">output index</param>
        /// <returns></returns>
        public static BigInteger CalculatePosition(ulong blkNum, UInt16 txIndex, UInt16 oIndex)
        {
            return (blkNum * new BigInteger(1000000000)) + (txIndex * new BigInteger(10000)) + oIndex;
        }
    }
}
