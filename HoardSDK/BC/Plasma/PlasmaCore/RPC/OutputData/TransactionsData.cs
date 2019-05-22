using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PlasmaCore.RPC.OutputData
{
    public class TransactionsData
    {
        public class ResultData
        {
            [JsonProperty(propertyName: "currency")]
            public string Currency { get; private set; }

            [JsonProperty(propertyName: "value")]
            public BigInteger Value { get; private set; }
        }

        [JsonProperty(propertyName: "block")]
        public BlockData Block { get; private set; }

        [JsonProperty(propertyName: "txindex")]
        public UInt16 TxIndex { get; private set; }

        [JsonProperty(propertyName: "txhash")]
        public string TxHash { get; private set; }

        [JsonProperty(propertyName: "results")]
        public ResultData[] Results { get; private set; }
    }
}
