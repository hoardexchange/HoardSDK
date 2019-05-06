using Hoard.Utils;
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
        /// <param name="profileFrom">account used to sign transaction</param>
        /// <returns>signed transaction as string</returns>
        public async Task<string> Sign(Profile profileFrom)
        {
            var encodedData = GetRLPEncoded();

            var signedTransaction = await profileFrom.SignTransaction(encodedData);

            var decodedList = RLP.Decode(signedTransaction.HexToByteArray());
            var decodedRlpCollection = (RLPCollection)decodedList[0];

            return EthECDSASignature.CreateStringSignature(
                EthECDSASignatureFactory.FromComponents(
                    decodedRlpCollection[2].RLPData, 
                    decodedRlpCollection[3].RLPData, 
                    decodedRlpCollection[1].RLPData
                    )
                );
        }

        /// <summary>
        /// Build RLP encoded signed transaction
        /// </summary>
        /// <param name="signatures">list of signatures</param>
        /// <returns>RLP encoded signed transaction</returns>
        public byte[] GetRLPEncoded(List<string> signatures = null)
        {
            var rlpcollection = new List<byte[]>();

            if (signatures != null)
            {
                var rlpSignatures = new List<byte[]>();
                signatures.ForEach(signature => rlpSignatures.Add(RLP.EncodeElement(signature.HexToByteArray())));

                rlpcollection.Add(RLP.EncodeList(rlpSignatures.ToArray()));
            }

            var rlpInputs = new List<byte[]>();
            foreach (var utxo in inputs)
            {
                rlpInputs.Add(RLP.EncodeList(utxo.GetRLPEncoded().ToArray()));
            }
            rlpcollection.Add(RLP.EncodeList(rlpInputs.ToArray()));

            var rlpOutputs = new List<byte[]>();
            foreach (var output in outputs)
            {
                rlpOutputs.Add(RLP.EncodeList(output.GetRLPEncoded().ToArray()));
            }
            rlpcollection.Add(RLP.EncodeList(rlpOutputs.ToArray()));

            return RLP.EncodeList(rlpcollection.ToArray());
        }
    }
}
