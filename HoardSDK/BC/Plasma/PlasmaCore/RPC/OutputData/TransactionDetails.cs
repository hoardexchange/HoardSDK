using Newtonsoft.Json;
using PlasmaCore.UTXO;
using System;

namespace PlasmaCore.RPC.OutputData
{
    public class TransactionDetails
    {
        [JsonProperty(propertyName: "txindex")]
        public UInt16 TxIndex { get; private set; }

        [JsonProperty(propertyName: "txhash")]
        public string TxHash { get; private set; }

        [JsonProperty(propertyName: "txbytes")]
        public string TxBytes { get; private set; }

        [JsonProperty(propertyName: "block")]
        public BlockData Block { get; private set; }

        [JsonProperty(propertyName: "inputs")]
        public UTXOData[] Inputs { get; private set; }

        [JsonProperty(propertyName: "outputs")]
        public UTXOData[] Outputs { get; private set; }
    }
}
