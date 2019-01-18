using Newtonsoft.Json;
using System.Numerics;

namespace Hoard.BC.Plasma
{
    public class UTXOData
    {
        [JsonProperty(propertyName: "txindex")]
        public BigInteger TxIndex { get; private set; }

        [JsonProperty(propertyName: "txbytes")]
        public string TxBytes { get; private set; }

        [JsonProperty(propertyName: "oindex")]
        public BigInteger OIndex { get; private set; }

        [JsonProperty(propertyName: "currency")]
        public string Currency { get; private set; }

        [JsonProperty(propertyName: "blknum")]
        public BigInteger BlkNum { get; private set; }

        [JsonProperty(propertyName: "amount")]
        public BigInteger Amount { get; private set; }

        public UTXOData(BigInteger amount)
        {
            Currency = null;
            Amount = amount;
        }
    }
}
