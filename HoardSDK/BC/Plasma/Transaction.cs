using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Plasma
{
    /// <summary>
    /// Plasma transaction
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Null signature
        /// </summary>
        public static string NULL_SIGNATURE = "0x0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";

        /// <summary>
        /// List of transaction input UTXOs
        /// </summary>
        private List<UTXOData> inputs = new List<UTXOData>();

        /// <summary>
        /// List of transaction output data
        /// </summary>
        private List<TransactionOutputData> outputs = new List<TransactionOutputData>();

        /// <summary>
        /// Gets a number of inputs
        /// </summary>
        /// <returns></returns>
        public UInt32 GetInputCount()
        {
            return (UInt32)inputs.Count;
        }

        /// <summary>
        /// Gets a number of outputs
        /// </summary>
        /// <returns></returns>
        public UInt32 GetOutputCount()
        {
            return (UInt32)outputs.Count;
        }

        /// <summary>
        /// Adds input to transaction
        /// </summary>
        /// <param name="data">transaction UTXO input data</param>
        public void AddInput(UTXOData data)
        {
            inputs.Add(data);
        }

        /// <summary>
        /// Adds output to transaction
        /// </summary>
        /// <param name="data">transaction output data</param>
        public void AddOutput(TransactionOutputData data)
        {
            outputs.Add(data);
        }

        /// <summary>
        /// Builds transaction object from provided inputs and outputs and signs it with account info
        /// </summary>
        /// <param name="fromAccount">account used to sign transaction</param>
        /// <returns>signed transaction as string</returns>
        public async Task<string> Sign(AccountInfo fromAccount)
        {
            var txData = PrepareTransactionData();
            var encodedData = RLPEncoder.EncodeData(txData.ToArray());
            return await fromAccount.SignTransaction(encodedData);
        }

        /// <summary>
        /// Build RLP encoded signed transaction
        /// </summary>
        /// <param name="signatures">list of signatures</param>
        /// <returns>RLP encoded signed transaction</returns>
        public string GetRLPEncoded(List<string> signatures)
        {
            var txData = PrepareTransactionData();
            signatures.ForEach(signature => txData.Add(signature.HexToByteArray()));
            return RLPEncoder.EncodeData(txData.ToArray()).ToHex().ToLower();
        }

        /// <summary>
        /// Builds transaction object from provided inputs and outputs
        /// </summary>
        /// <returns>transaction object as bytes</returns>
        protected List<byte[]> PrepareTransactionData()
        {
            var txData = new List<byte[]>();
            txData.Clear();

            //TESUJI_PLASMA
            foreach (var utxo in inputs)
            {
                txData.AddRange(utxo.GetTxBytes());
            }

            txData.Add(inputs[0].Currency.HexToByteArray());
            
            foreach (var output in outputs)
            {
                txData.AddRange(output.GetTxBytes());
            }
            //TESUJI_PLASMA

            return txData;
        }
    }
}
