using Newtonsoft.Json;
using System.Numerics;

namespace Hoard.BC.Plasma
{
    public class TransactionData
    {
        [JsonProperty(propertyName: "txindex2")]
        public BigInteger TxIndex2 { get; private set; }

        [JsonProperty(propertyName: "txindex1")]
        public BigInteger TxIndex1 { get; private set; }

        [JsonProperty(propertyName: "txindex")]
        public BigInteger TxIndex { get; private set; }

        [JsonProperty(propertyName: "txid")]
        public BigInteger TxId { get; private set; }

        [JsonProperty(propertyName: "txblknum")]
        public BigInteger TxBlkNum { get; private set; }

        [JsonProperty(propertyName: "timestamp")]
        public BigInteger TimeStamp { get; private set; }

        [JsonProperty(propertyName: "spender2")]
        public string Spender2 { get; private set; }

        [JsonProperty(propertyName: "spender1")]
        public string Spender1 { get; private set; }

        [JsonProperty(propertyName: "sig2")]
        public string Sig2 { get; private set; }

        [JsonProperty(propertyName: "sig1")]
        public string Sig1 { get; private set; }

        [JsonProperty(propertyName: "oindex2")]
        public BigInteger OIndex2 { get; private set; }

        [JsonProperty(propertyName: "oindex1")]
        public BigInteger OIndex1 { get; private set; }

        [JsonProperty(propertyName: "newowner2")]
        public string NewOwner2 { get; private set; }

        [JsonProperty(propertyName: "newowner1")]
        public string NewOwner1 { get; private set; }

        [JsonProperty(propertyName: "eth_height")]
        public BigInteger EthHeight { get; private set; }

        [JsonProperty(propertyName: "cur12")]
        public string Cur12 { get; private set; }

        [JsonProperty(propertyName: "blknum2")]
        public BigInteger BlkNum2 { get; private set; }

        [JsonProperty(propertyName: "blknum1")]
        public BigInteger BlkNum1 { get; private set; }

        [JsonProperty(propertyName: "amount2")]
        public BigInteger Amount2 { get; private set; }

        [JsonProperty(propertyName: "amount1")]
        public BigInteger Amount1 { get; private set; }
    }
}
