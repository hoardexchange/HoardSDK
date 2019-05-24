using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using System;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for submit transaction request
    /// </summary>
    public class TransactionReceipt
    {
        /// <summary>
        /// Block number
        /// </summary>
        [JsonProperty(propertyName: "blknum")]
        public UInt64 BlkNum { get; private set; }

        /// <summary>
        /// Transaction index in the block
        /// </summary>
        [JsonProperty(propertyName: "txindex")]
        public UInt16 TxIndex { get; private set; }

        /// <summary>
        /// Transaction hash
        /// </summary>
        [JsonProperty(propertyName: "txhash")]
        public HexBigInteger TxHash { get; private set; }
    }
}
