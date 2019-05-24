using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using PlasmaCore.RPC.OutputData;
using PlasmaCore.Transactions;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace PlasmaCore.UTXO
{
    public class FCConsolidator
    {
        public List<UTXOData> MergedUtxos { get; protected set; }

        public List<Transaction> Transactions { get; protected set; }

        // if both CanMerge and AllConsolidated are false at the same time - something went wrong during transaction submition
        public bool CanMerge { get; protected set; }

        public bool AllConsolidated
        {
            get
            {
                foreach(var utxo in MergedUtxos)
                {
                    if (!balances.ContainsKey(utxo.Currency) || balances[utxo.Currency] != (utxo as FCUTXOData).Amount)
                        return false;
                }
                return true;
            }
        }

        private PlasmaAPIService plasmaAPIService = null;

        private Dictionary<string, List<UTXOData>> utxoDict = new Dictionary<string, List<UTXOData>>();

        private Dictionary<string, BigInteger> balances = new Dictionary<string, BigInteger>();

        private string owner;

        public FCConsolidator(PlasmaAPIService _plasmaAPIService, string _owner, UTXOData[] _utxos)
        {
            Transactions = new List<Transaction>();
            MergedUtxos = new List<UTXOData>();
            plasmaAPIService = _plasmaAPIService;
            owner = _owner;

            foreach (var utxo in _utxos)
            {
                if ((utxo is FCUTXOData) && (utxo.Owner == owner))
                {
                    if (!utxoDict.ContainsKey(utxo.Currency))
                    {
                        utxoDict.Add(utxo.Currency, new List<UTXOData>());
                        balances.Add(utxo.Currency, BigInteger.Zero);
                    }
                    utxoDict[utxo.Currency].Add(utxo);
                    balances[utxo.Currency] += (utxo as FCUTXOData).Amount;
                }
            }

            PrepareTransactions();
        }

        public async Task ProcessTransactions()
        {
            if (CanMerge)
            {
                foreach (var transaction in Transactions)
                {
                    TransactionReceipt receipt = await plasmaAPIService.SubmitTransaction(transaction.GetRLPEncoded().ToHex(true));
                    if (receipt != null)
                    {
                        FCUTXOData utxo = new FCUTXOData();
                        utxo.BlkNum = receipt.BlkNum;
                        utxo.TxIndex = receipt.TxIndex;
                        utxo.OIndex = 0;
                        utxo.Amount = RLP.Decode(transaction.Outputs[0].RLPEncodedValue)[0].RLPData.ToBigIntegerFromRLPDecoded();
                        utxo.Owner = transaction.Outputs[0].Owner;
                        utxo.Currency = transaction.Outputs[0].Currency;

                        utxoDict[transaction.Outputs[0].Currency].Add(utxo);
                    }
                }

                Transactions.Clear();

                PrepareTransactions();
            }
        }

        private void PrepareTransactions()
        {
            foreach (var utxoElem in utxoDict.ToList())
            {
                utxoDict[utxoElem.Key] = new List<UTXOData>();

                string currency = utxoElem.Key;
                if (utxoElem.Value.Count == 1)
                {
                    MergedUtxos.Add(utxoElem.Value[0]);
                }
                else if(utxoElem.Value.Count > 1)
                {
                    var splitUtxo = Split(utxoElem.Value, 4);
                    foreach (var utxoGroup in splitUtxo)
                    {
                        if (utxoGroup.Count > 1)
                        {
                            BigInteger sum = BigInteger.Zero;
                            Transaction tx = new Transaction();
                            utxoGroup.ForEach(x =>
                            {
                                tx.AddInput(x);
                                sum += (x as FCUTXOData).Amount;
                            });
                            tx.AddOutput(owner, currency, sum);

                            Transactions.Add(tx);
                        }
                        else
                        {
                            utxoDict[currency].Add(utxoGroup[0]);
                        }
                    }
                }
            }
            CanMerge = (Transactions.Count > 0);
        }

        private static List<List<T>> Split<T>(List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
