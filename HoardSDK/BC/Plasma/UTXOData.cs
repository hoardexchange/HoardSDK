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
    public abstract class UTXOData
    {
        /// <summary>
        /// Transaction index within the block
        /// </summary>
        [JsonProperty(propertyName: "txindex")]
        public BigInteger TxIndex { get; private set; }

        /// <summary>
        /// Transaction hash
        /// </summary>
        [JsonProperty(propertyName: "txbytes")]
        public string TxBytes { get; private set; }

        /// <summary>
        /// Transaction output index
        /// </summary>
        [JsonProperty(propertyName: "oindex")]
        public BigInteger OIndex { get; private set; }

        /// <summary>
        /// Currency of the transaction (all zeroes for ETH)
        /// </summary>
        [JsonProperty(propertyName: "currency")]
        public string Currency { get; private set; }

        /// <summary>
        /// Block number
        /// </summary>
        [JsonProperty(propertyName: "blknum")]
        public BigInteger BlkNum { get; private set; }
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
        public BigInteger Amount { get; private set; }

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
                    UInt32 idxL = 0, idxR = utxosCount;
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

        //static public List<byte[]> CreateTransaction(AccountInfo fromAccount, string toAddress, BigInteger amount, List<ERC223UTXOData> inputUtxos)
        //{
        //    Debug.Assert(inputUtxos != null);
        //    Debug.Assert(inputUtxos.Count <= 2);

        //    // create transaction data
        //    var txData = new List<byte[]>();
        //    for (UInt16 i = 0; i < 2/*MAX_INPUTS*/; ++i)
        //    {
        //        if (i < inputUtxos.Count())
        //        {
        //            // cannot mix currencies
        //            Debug.Assert(inputUtxos[0].Currency == inputUtxos[i].Currency);

        //            txData.Add(inputUtxos[i].BlkNum.ToBytesForRLPEncoding());
        //            txData.Add(inputUtxos[i].TxIndex.ToBytesForRLPEncoding());
        //            txData.Add(inputUtxos[i].OIndex.ToBytesForRLPEncoding());
        //        }
        //        else
        //        {
        //            txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
        //            txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
        //            txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
        //        }
        //    }

        //    txData.Add(inputUtxos[0].Currency.HexToByteArray());

        //    txData.Add(toAddress.HexToByteArray());
        //    txData.Add(amount.ToBytesForRLPEncoding());

        //    var sum = new BigInteger(0);
        //    inputUtxos.ForEach(x => sum += x.Amount);
        //    if (sum > amount)
        //    {
        //        txData.Add(fromAccount.ID.ToHexByteArray());
        //        txData.Add((sum - amount).ToBytesForRLPEncoding());
        //    }
        //    else
        //    {
        //        txData.Add("0x0000000000000000000000000000000000000000".HexToByteArray());
        //        txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
        //    }

        //    return txData;
        //}
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
        public List<BigInteger> TokenIds { get; private set; }
    }

    /// <summary>
    /// Factory of UTXO data
    /// </summary>
    public class UTXODataFactory
    {
        /// <summary>
        /// Based on json content it creates typed UTXOData (ERC223, ERC721)
        /// </summary>
        /// <param name="jsonData">json UTXO data</param>
        /// <returns>typed UTXOData</returns>
        public static UTXOData Deserialize(string jsonData)
        {
            JObject obj = JObject.Parse(jsonData);
            if(obj["amount"] != null)
            {
                return JsonConvert.DeserializeObject<ERC223UTXOData>(jsonData);
            }
            else if(obj["tokenid"] != null)
            {
                return JsonConvert.DeserializeObject<ERC721UTXOData>(jsonData);
            }
            else
            {
                //TODO not supported utxo format
                throw new NotImplementedException();
            }
        }
    }
}
