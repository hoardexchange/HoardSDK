﻿using Newtonsoft.Json;
using System;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for transaction requests
    /// </summary>
    public class BlockData
    {
        /// <summary>
        /// Timestamp when block was mined
        /// </summary>
        [JsonProperty(propertyName: "timestamp")]
        public UInt64 Timestamp { get; private set; }

        /// <summary>
        /// Mined block hash
        /// </summary>
        [JsonProperty(propertyName: "hash")]
        public string Hash { get; private set; }

        /// <summary>
        /// Ethereum height block was published on
        /// </summary>
        [JsonProperty(propertyName: "eth_height")]
        public UInt64 EthHeight { get; private set; }

        /// <summary>
        /// Block number
        /// </summary>
        [JsonProperty(propertyName: "blknum")]
        public UInt64 Blknum { get; private set; }
    }
}