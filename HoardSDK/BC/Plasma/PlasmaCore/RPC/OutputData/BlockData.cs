using Newtonsoft.Json;
using System;

namespace PlasmaCore.RPC.OutputData
{
    public class BlockData
    {
        [JsonProperty(propertyName: "timestamp")]
        public UInt64 Timestamp { get; private set; }

        [JsonProperty(propertyName: "hash")]
        public string Hash { get; private set; }

        [JsonProperty(propertyName: "eth_height")]
        public UInt64 EthHeight { get; private set; }

        [JsonProperty(propertyName: "blknum")]
        public UInt64 Blknum { get; private set; }
    }
}
