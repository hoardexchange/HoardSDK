using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.Transaction
{
    /// <summary>
    /// Description of transaction data
    /// </summary>
    public class TransactionData
    {
        /// <summary>
        /// Index of second input transaction within the block
        /// </summary>
        [JsonProperty(propertyName: "txindex2")]
        public BigInteger TxIndex2 { get; private set; }

        /// <summary>
        /// Index of first input transaction within the block
        /// </summary>
        [JsonProperty(propertyName: "txindex1")]
        public BigInteger TxIndex1 { get; private set; }

        /// <summary>
        /// Index of outcome transaction within the block
        /// </summary>
        [JsonProperty(propertyName: "txindex")]
        public UInt16 TxIndex { get; private set; }

        /// <summary>
        /// Outcome transaction hash
        /// </summary>
        [JsonProperty(propertyName: "txid")]
        public BigInteger TxId { get; private set; }

        /// <summary>
        /// Outcome transaction block number
        /// </summary>
        [JsonProperty(propertyName: "txblknum")]
        public BigInteger TxBlkNum { get; private set; }

        /// <summary>
        /// Outcome transaction timestamp
        /// </summary>
        [JsonProperty(propertyName: "timestamp")]
        public UInt64 TimeStamp { get; private set; }

        /// <summary>
        /// Owner of second input transaction
        /// </summary>
        [JsonProperty(propertyName: "spender2")]
        public string Spender2 { get; private set; }

        /// <summary>
        /// Owner of first input transaction
        /// </summary>
        [JsonProperty(propertyName: "spender1")]
        public string Spender1 { get; private set; }

        /// <summary>
        /// Second signature
        /// </summary>
        [JsonProperty(propertyName: "sig2")]
        public string Sig2 { get; private set; }

        /// <summary>
        /// First signature
        /// </summary>
        [JsonProperty(propertyName: "sig1")]
        public string Sig1 { get; private set; }

        /// <summary>
        /// Second transaction output index
        /// </summary>
        [JsonProperty(propertyName: "oindex2")]
        public BigInteger OIndex2 { get; private set; }

        /// <summary>
        /// First transaction output index
        /// </summary>
        [JsonProperty(propertyName: "oindex1")]
        public BigInteger OIndex1 { get; private set; }

        /// <summary>
        /// Second receiver address
        /// </summary>
        [JsonProperty(propertyName: "newowner2")]
        public string NewOwner2 { get; private set; }

        /// <summary>
        /// First receiver address
        /// </summary>
        [JsonProperty(propertyName: "newowner1")]
        public string NewOwner1 { get; private set; }

        /// <summary>
        /// Ethereum chain height where the block got submitted
        /// </summary>
        [JsonProperty(propertyName: "eth_height")]
        public UInt64 EthHeight { get; private set; }

        /// <summary>
        /// Currency of the transaction
        /// </summary>
        [JsonProperty(propertyName: "cur12")]
        public string Cur12 { get; private set; }

        /// <summary>
        /// Block number of the second input transaction within the child chain
        /// </summary>
        [JsonProperty(propertyName: "blknum2")]
        public BigInteger BlkNum2 { get; private set; }

        /// <summary>
        /// Block number of the first input transaction within the child chain
        /// </summary>
        [JsonProperty(propertyName: "blknum1")]
        public BigInteger BlkNum1 { get; private set; }

        /// <summary>
        /// Amount transfered to the second new owner
        /// </summary>
        [JsonProperty(propertyName: "amount2")]
        public BigInteger Amount2 { get; private set; }

        /// <summary>
        /// Amount transfered to the first new owner
        /// </summary>
        [JsonProperty(propertyName: "amount1")]
        public BigInteger Amount1 { get; private set; }
    }
}
