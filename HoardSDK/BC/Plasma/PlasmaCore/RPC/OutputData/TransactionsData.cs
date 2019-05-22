using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for transaction data request
    /// </summary>
    public class TransactionsData
    {
        /// <summary>
        /// Transaction result data
        /// </summary>
        public class ResultData
        {
            /// <summary>
            /// Data currency
            /// </summary>
            [JsonProperty(propertyName: "currency")]
            public string Currency { get; private set; }

            /// <summary>
            /// Data value
            /// </summary>
            [JsonProperty(propertyName: "value")]
            public BigInteger Value { get; private set; }
        }

        /// <summary>
        /// Transaction block data
        /// </summary>
        [JsonProperty(propertyName: "block")]
        public BlockData Block { get; private set; }

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
        /// Tranasction data
        /// </summary>
        [JsonProperty(propertyName: "results")]
        public ResultData[] Results { get; private set; }
    }
}
