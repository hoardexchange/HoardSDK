using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Plasma
{
    public class Transaction
    {
        private static string NULL_SIGNATURE = "0x0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";

        /// <summary>
        /// Mapping currency (address) to list of UTXOs
        /// </summary>
        private Dictionary<string, List<UTXOData>> inputs;

        /// <summary>
        /// Mapping currency (address) to list of UTXOs
        /// </summary>
        private Dictionary<string, BigInteger> outputs;

        /// <summary>
        /// Adds input to transaction
        /// </summary>
        /// <param name="data">UTXO input data</param>
        public void AddInput(UTXOData data)
        {
            if (inputs.ContainsKey(data.Currency))
            {
                inputs[data.Currency].Add(data);
            }
            else
            {
                inputs.Add(data.Currency, new List<UTXOData>() { data });
            }
        }

        /// <summary>
        /// Adds output to transaction
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="data"></param>
        public void AddOutput(string toAddress, BigInteger data/*, string currency*/)
        {
            outputs.Add(toAddress, data);
        }

        /// <summary>
        /// Builds transaction object from provided inputs and outputs and signs it with account info
        /// </summary>
        /// <param name="fromAccount">account used to sign transaction</param>
        /// <returns>signed transaction as string</returns>
        public async Task<string> Sign(AccountInfo fromAccount)
        {
            var txData = PrepareTransactionData();

            var signer = new RLPSigner(txData.ToArray());

            var signature = await fromAccount.SignTransaction(signer.GetRLPEncodedRaw());

            txData.Add(signature.HexToByteArray());
            txData.Add(NULL_SIGNATURE.HexToByteArray());
            
            signer = new RLPSigner(txData.ToArray());

            return signer.GetRLPEncodedRaw().ToHex().ToLower();
        }

        /// <summary>
        /// Builds transaction object from provided inputs and outputs
        /// </summary>
        /// <returns>transaction object as bytes</returns>
        protected List<byte[]> PrepareTransactionData()
        {
            var txData = new List<byte[]>();
            txData.Clear();

            foreach (var input in inputs)
            {
                foreach (var utxo in input.Value)
                {
                    txData.Add(utxo.BlkNum.ToBytesForRLPEncoding());
                    txData.Add(utxo.TxIndex.ToBytesForRLPEncoding());
                    txData.Add(utxo.OIndex.ToBytesForRLPEncoding());
                }
            }

            // FIXME it depends from plasma transaction api - it probably change in the future
            //txData.Add(inputs.Currency.HexToByteArray());

            foreach (var output in outputs)
            {
                txData.Add(output.Key.HexToByteArray());
                txData.Add(output.Value.ToBytesForRLPEncoding());
            }

            return txData;
        }
    }
}
