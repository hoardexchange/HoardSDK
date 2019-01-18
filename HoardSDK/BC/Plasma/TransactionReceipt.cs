using Newtonsoft.Json;
using System.Numerics;

namespace Hoard.BC.Plasma
{
    public class TransactionReceipt
    {
        [JsonProperty(propertyName: "blknum")]
        public BigInteger BlkNum { get; private set; }

        [JsonProperty(propertyName: "tx_index")]
        public BigInteger TxIndex { get; private set; }

        [JsonProperty(propertyName: "tx_hash")]
        public BigInteger TxHash { get; private set; }
    }
}
