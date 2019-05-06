using Nethereum.RLP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Hoard.BC.Plasma
{
    /// <summary>
    /// Description of Plasma Unspent Transaction Output 
    /// </summary>
    [JsonConverter(typeof(UTXOConverter))]
    public class UTXOData
    {
        /// <summary>
        /// Transaction index within the block
        /// </summary>
        [JsonProperty(propertyName: "txindex")]
        public BigInteger TxIndex { get; protected set; }

        /// <summary>
        /// Transaction hash
        /// </summary>
        [JsonProperty(propertyName: "txbytes")]
        public string TxBytes { get; protected set; }

        /// <summary>
        /// Transaction output index
        /// </summary>
        [JsonProperty(propertyName: "oindex")]
        public BigInteger OIndex { get; protected set; }

        /// <summary>
        /// Currency of the transaction (all zeroes for ETH)
        /// </summary>
        [JsonProperty(propertyName: "currency")]
        public string Currency { get; protected set; }

        /// <summary>
        /// Block number
        /// </summary>
        [JsonProperty(propertyName: "blknum")]
        public BigInteger BlkNum { get; protected set; }

        /// <summary>
        /// Gets empty utxo
        /// </summary>
        public static UTXOData Empty
        {
            get
            {
                var utxoData = new UTXOData();
                utxoData.BlkNum = BigInteger.Zero;
                utxoData.TxIndex = BigInteger.Zero;
                utxoData.OIndex = BigInteger.Zero;
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
            data.Add(RLP.EncodeElement(BlkNum.ToBytesForRLPEncoding()));
            data.Add(RLP.EncodeElement(TxIndex.ToBytesForRLPEncoding()));
            data.Add(RLP.EncodeElement(OIndex.ToBytesForRLPEncoding()));
            return data;
        }
    }

    /// <summary>
    /// Description of Plasma ERC223 Unspent Transaction Output 
    /// </summary>
    public class ERC223UTXOData : UTXOData
    {
        private static BigInteger U256_MAX_VALUE = BigInteger.Pow(2, 256);

        /// <summary>
        /// Amount of tokens
        /// </summary>
        [JsonProperty(propertyName: "amount")]
        public BigInteger Amount { get; protected set; }

        /// <summary>
        /// Finds optimal collection of UTXOs that amount sum is equal or greater than given target amount using selection algorithm
        /// </summary>
        /// <param name="utxos">list of UTXO to filter from</param>
        /// <param name="targetAmount">target amount</param>
        /// <returns></returns>
        static public List<ERC223UTXOData> FindInputs(List<UTXOData> utxos, BigInteger targetAmount)
        {
            var erc223Utxos = utxos.OfType<ERC223UTXOData>().ToList();

            Debug.Assert(erc223Utxos.Count == utxos.Count);

            erc223Utxos.RemoveAll(x => x.Amount == 0);
            erc223Utxos.Insert(0, new ERC223UTXOData());

            var sortedERC223Utxos = erc223Utxos.OrderBy(x => x.Amount).ToArray();

            // TODO: simple utxo selection algorithm - assuption return max 2 inputs

            var utxosCount = (UInt32)sortedERC223Utxos.Count();
            if (utxosCount > 1)
            {
                // find single utxo or pair of utxos closest to given amount
                if (sortedERC223Utxos[utxosCount - 1].Amount + sortedERC223Utxos[utxosCount - 2].Amount >= targetAmount)
                {
                    UInt32 resultIdxL = 0, resultIdxR = 0;
                    UInt32 idxL = 0, idxR = utxosCount - 1;
                    var diff = U256_MAX_VALUE;

                    while (idxR > idxL)
                    {
                        var utxoSum = sortedERC223Utxos[idxL].Amount + sortedERC223Utxos[idxR].Amount;
                        if (utxoSum >= targetAmount && utxoSum - targetAmount < diff)
                        {
                            resultIdxL = idxL;
                            resultIdxR = idxR;
                            diff = utxoSum - targetAmount;
                        }

                        if (utxoSum > targetAmount)
                        {
                            idxR--;
                        }
                        else
                        {
                            idxL++;
                        }
                    }

                    Debug.Assert(sortedERC223Utxos[resultIdxL].Amount + sortedERC223Utxos[resultIdxR].Amount >= targetAmount);

                    if (resultIdxL > 0)
                        return new List<ERC223UTXOData>() { sortedERC223Utxos[resultIdxL], sortedERC223Utxos[resultIdxR] };
                    return new List<ERC223UTXOData>() { sortedERC223Utxos[resultIdxR] };

                }
                else
                {
                    var sum = new BigInteger(0);
                    erc223Utxos.ForEach(x => sum += x.Amount);

                    if (sum >= targetAmount)
                    {
                        //TODO merge utxos and check if it is possible to find pair of utxo that is greater or equal than given amount
                        throw new NotImplementedException();
                    }
                }
            }

            // no sufficient funds
            return null;
        }
    }

    /// <summary>
    /// Description of Plasma ERC721 Unspent Transaction Output 
    /// </summary>
    public class ERC721UTXOData : UTXOData
    {
        /// <summary>
        /// List of token ids
        /// </summary>
        [JsonProperty(propertyName: "tokenid")]
        public List<BigInteger> TokenIds { get; protected set; }
    }

    internal class UTXOConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jObject = JToken.ReadFrom(reader);

            UTXOData result = null;
            if (jObject["amount"] != null)
                result = new ERC223UTXOData();
            else if (jObject["tokenid"] != null)
                result = new ERC721UTXOData();
            else
            {
                //TODO not supported utxo format
                throw new NotImplementedException();
            }

            serializer.Populate(jObject.CreateReader(), result);

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
