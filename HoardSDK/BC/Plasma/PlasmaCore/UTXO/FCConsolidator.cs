using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using PlasmaCore.RPC.OutputData;
using PlasmaCore.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace PlasmaCore.UTXO
{
    /// <summary>
    /// Fungible currency consolidator class (use only if operator is trusted) 
    /// </summary>
    public class FCConsolidator
    {
        /// <summary>
        /// Result utxo (merged)
        /// </summary>
        public UTXOData MergedUtxo { get; protected set; }

        /// <summary>
        /// Transaction pending to submit
        /// </summary>
        public List<Transaction> Transactions { get; protected set; }

        // if both CanMerge and AllConsolidated are false at the same time - something went wrong during transaction submition
        /// <summary>
        /// Returns if data still needs consolidation
        /// </summary>
        public bool CanMerge { get; protected set; }

        /// <summary>
        /// Checks if all data was consolidated - input and output data have equal balances
        /// </summary>
        public bool AllConsolidated
        {
            get
            {
                return (MergedUtxo != null) && (amount == (MergedUtxo as FCUTXOData).Amount);
            }
        }

        private PlasmaAPIService plasmaAPIService = null;

        private List<UTXOData> utxoList = new List<UTXOData>();

        private BigInteger balance = BigInteger.Zero;

        private string owner;
        private string currency;
        private BigInteger amount;
        private ITransactionEncoder txEncoder;

        /// <summary>
        /// Creates default fungible currency consolidator
        /// </summary>
        /// <param name="_plasmaAPIService">plasma API service</param>
        /// <param name="_txEncoder">transaction encoder</param>
        /// <param name="_owner">consolidation requester address</param>
        /// <param name="_currency">consolidation currency</param>
        /// <param name="_utxos">array of utxo data for consolidation</param>
        /// <param name="_amount">amount to consolidate (optional, if null or greater than balance consolidate all utxos)</param>
        public FCConsolidator(PlasmaAPIService _plasmaAPIService, ITransactionEncoder _txEncoder, string _owner, string _currency, UTXOData[] _utxos, BigInteger? _amount = null)
        {
            Transactions = new List<Transaction>();
            MergedUtxo = null;
            plasmaAPIService = _plasmaAPIService;
            owner = _owner;
            currency = _currency;
            txEncoder = _txEncoder;

            BigInteger balance = BigInteger.Zero;
            Array.Sort(_utxos, (x, y) => (x as FCUTXOData).Amount.CompareTo((x as FCUTXOData).Amount));
            foreach (var utxo in _utxos)
            {
                if ((utxo is FCUTXOData) && (utxo.Owner == owner) && (utxo.Currency == _currency))
                {
                    utxoList.Add(utxo);
                    balance += (utxo as FCUTXOData).Amount;
                    if(_amount.HasValue && balance >= _amount.Value)
                    {
                        break;
                    }
                }
            }

            amount = (_amount.HasValue && balance >= _amount.Value) ? _amount.Value : balance;

            PrepareTransactions();
        }

        /// <summary>
        /// Submits signed transactions and queue response utxos
        /// </summary>
        /// <returns></returns>
        public async Task ProcessTransactions()
        {
            if (CanMerge)
            {
                foreach (var transaction in Transactions)
                {
                    string encodedTx = txEncoder.EncodeSigned(transaction).ToHex(true);
                    TransactionReceipt receipt = await plasmaAPIService.SubmitTransaction(encodedTx);
                    if (receipt != null)
                    {
                        FCUTXOData utxo = new FCUTXOData();
                        utxo.BlkNum = receipt.BlkNum;
                        utxo.TxIndex = receipt.TxIndex;
                        utxo.OIndex = 0;
                        utxo.Amount = transaction.Outputs[0].Value.ToBigIntegerFromRLPDecoded();
                        utxo.Owner = transaction.Outputs[0].Owner;
                        utxo.Currency = transaction.Outputs[0].Currency;
                        utxo.Position = UTXOData.CalculatePosition(utxo.BlkNum, utxo.TxIndex, utxo.OIndex);
                        utxoList.Add(utxo);
                    }
                }

                Transactions.Clear();

                PrepareTransactions();
            }
        }

        private void PrepareTransactions()
        {
            var pendingUtxos = new List<UTXOData>();

            if (utxoList.Count == 1)
            {
                var fcutxo = (utxoList[0] as FCUTXOData);
                if (fcutxo.Amount > amount)
                {
                    Transaction tx = new Transaction();
                    tx.AddInput(utxoList[0]);
                    tx.AddOutput(owner, currency, amount);
                    tx.AddOutput(owner, currency, fcutxo.Amount - amount);
                    Transactions.Add(tx);
                }
                else if (fcutxo.Amount == amount)
                {
                    MergedUtxo = utxoList[0];
                }
            }
            else if(utxoList.Count > 1)
            {
                var splitUtxo = Split(utxoList, 4);
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
                        pendingUtxos.Add(utxoGroup[0]);
                    }
                }
            }

            utxoList.Clear();
            utxoList.AddRange(pendingUtxos);

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
