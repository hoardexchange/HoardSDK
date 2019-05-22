using PlasmaCore.UTXO;
using System;
using System.Numerics;

namespace PlasmaCore.Transaction
{
    /// <summary>
    /// Plasma transaction builder helper class
    /// </summary>
    public static class FCTransactionBuilder
    {
        /// <summary>
        /// Builds the simplest, the most common fungible transaction (one fungible currency, one receiver, one sender)
        /// </summary>
        /// <param name="addrees">account of tokens owner</param>
        /// <param name="addressTo">address of destination account</param>
        /// <param name="utxos">input utxos satisfing given amount</param>
        /// <param name="amount">amount of tokens to transfer</param>
        /// <param name="currency">currency of tokens to transfer</param>
        /// <returns>RLP encoded signed transaction</returns>
        public static Transaction Build(string addreesFrom, string addressTo, UTXOData[] utxos, BigInteger amount, string currency)
        {
            UTXOData[] inputUtxos = Array.FindAll(utxos, x => (x is FCUTXOData) && (x.Currency == currency));

            //TODO write more optimized algorithm to find utxo inputs
            Array.Sort<UTXOData>(inputUtxos, new Comparison<UTXOData>((x, y) => (x as FCUTXOData).Amount.CompareTo((y as FCUTXOData).Amount)));

            if (inputUtxos.Length == 0)
                throw new ArgumentException("Not enough tokens!");

            var tx = new Transaction();

            BigInteger sum = BigInteger.Zero;
            for(Int32 i = 0; i < inputUtxos.Length; ++i)
            {
                if (tx.AddInput(inputUtxos[i]))
                {
                    sum += (inputUtxos[i] as FCUTXOData).Amount;
                }
                else
                    break;
            }

            if (sum < amount)
                throw new ArgumentException("Not enough tokens!");

            tx.AddOutput(new FCTransactionOutputData(addressTo, currency, amount));
            if (sum > amount)
            {
                tx.AddOutput(new FCTransactionOutputData(addreesFrom, currency, sum - amount));
            }

            return tx;
        }
    }
}
