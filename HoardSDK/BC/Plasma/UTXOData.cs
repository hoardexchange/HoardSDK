using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Plasma
{
    public abstract class UTXOData
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
    }

    public class ERC223UTXOData : UTXOData
    {
        private static BigInteger U256_MAX_VALUE = BigInteger.Pow(2, 256);

        [JsonProperty(propertyName: "amount")]
        public BigInteger Amount { get; private set; }

        public ERC223UTXOData()
        {
            Amount = BigInteger.Zero;
        }

        static public List<ERC223UTXOData> FindInputs(HoardID account, BigInteger amount, List<UTXOData> utxos)
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
                if (sortedERC223Utxos[utxosCount - 1].Amount + sortedERC223Utxos[utxosCount - 2].Amount >= amount)
                {
                    UInt32 resultIdxL = 0, resultIdxR = 0;
                    UInt32 idxL = 0, idxR = utxosCount;
                    var diff = U256_MAX_VALUE;

                    while (idxR > idxL)
                    {
                        var utxoSum = sortedERC223Utxos[idxL].Amount + sortedERC223Utxos[idxR].Amount;
                        if (utxoSum >= amount && utxoSum - amount < diff)
                        {
                            resultIdxL = idxL;
                            resultIdxR = idxR;
                            diff = utxoSum - amount;
                        }

                        if (utxoSum > amount)
                        {
                            idxR--;
                        }
                        else
                        {
                            idxL++;
                        }
                    }

                    Debug.Assert(sortedERC223Utxos[resultIdxL].Amount + sortedERC223Utxos[resultIdxR].Amount >= amount);

                    if (resultIdxL > 0)
                        return new List<ERC223UTXOData>() { sortedERC223Utxos[resultIdxL], sortedERC223Utxos[resultIdxR] };
                    return new List<ERC223UTXOData>() { sortedERC223Utxos[resultIdxR] };

                }
                else
                {
                    var sum = new BigInteger(0);
                    erc223Utxos.ForEach(x => sum += x.Amount);

                    if (sum >= amount)
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

    public class ERC721UTXOData : UTXOData
    {
        [JsonProperty(propertyName: "tokenid")]
        public List<BigInteger> TokenIds { get; private set; }
    }

    public class UTXODataFactory
    {
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
