using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using System.Numerics;

namespace Hoard.BC.Plasma
{
    /// <summary>
    /// Description of Plasma transaction receipt
    /// </summary>
    public class TransactionReceipt
    {
        /// <summary>
        /// Block number
        /// </summary>
        [JsonProperty(propertyName: "blknum")]
        public BigInteger BlkNum { get; private set; }

        /// <summary>
        /// Transaction index in the block
        /// </summary>
        [JsonProperty(propertyName: "txindex")]
        public BigInteger TxIndex { get; private set; }

        /// <summary>
        /// Transaction hash
        /// </summary>
        [JsonProperty(propertyName: "txhash")]
        public HexBigInteger TxHash { get; private set; }
    }
}
