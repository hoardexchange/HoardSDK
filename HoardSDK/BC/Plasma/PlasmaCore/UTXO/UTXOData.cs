using Nethereum.RLP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace PlasmaCore.UTXO
{
    /// <summary>
    /// Description of Plasma Unspent Transaction Output 
    /// </summary>
    [JsonConverter(typeof(UTXOConverter))]
    public class UTXOData
    {
        public static readonly string ZERO_ADDRESS = "0x0000000000000000000000000000000000000000";

        /// <summary>
        /// Transaction index within the block
        /// </summary>
        [JsonProperty(propertyName: "txindex")]
        public UInt16 TxIndex { get; protected set; }

        /// <summary>
        /// Transaction output index
        /// </summary>
        [JsonProperty(propertyName: "oindex")]
        public UInt16 OIndex { get; protected set; }

        /// <summary>
        /// Currency of the transaction (all zeroes for ETH)
        /// </summary>
        [JsonProperty(propertyName: "currency")]
        public string Currency { get; protected set; }

        /// <summary>
        /// Block number
        /// </summary>
        [JsonProperty(propertyName: "blknum")]
        public UInt64 BlkNum { get; protected set; }

        /// <summary>
        /// UTXO position
        /// </summary>
        [JsonProperty(propertyName: "utxo_pos")]
        public BigInteger Position { get; protected set; }

        /// <summary>
        /// Owner of utxo
        /// </summary>
        [JsonProperty(propertyName: "owner")]
        public string Owner { get; protected set; }

        /// <summary>
        /// Gets empty utxo
        /// </summary>
        public static UTXOData Empty
        {
            get
            {
                var utxoData = new UTXOData();
                utxoData.BlkNum = 0;
                utxoData.TxIndex = 0;
                utxoData.OIndex = 0;
                utxoData.Position = BigInteger.Zero;
                utxoData.Owner = ZERO_ADDRESS;
                utxoData.Currency = ZERO_ADDRESS;
                return utxoData;
            }
        }

        /// <summary>
        /// Returns transaction input data
        /// </summary>
        /// <returns></returns>
        public virtual List<byte[]> GetRLPEncoded()
        {
            var data = new List<byte[]>();
            data.Add(RLP.EncodeElement(new BigInteger(BlkNum).ToBytesForRLPEncoding()));
            data.Add(RLP.EncodeElement(new BigInteger(TxIndex).ToBytesForRLPEncoding()));
            data.Add(RLP.EncodeElement(new BigInteger(OIndex).ToBytesForRLPEncoding()));
            return data;
        }
    }
}
