using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using PlasmaCore.UTXO;
using System;
using System.Collections.Generic;

namespace PlasmaCore.Transaction
{
    /// <summary>
    /// Plasma transaction
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Maximum transaction inputs count
        /// </summary>
        private static readonly Int32 MAX_INPUTS = 4;

        /// <summary>
        /// Maximum transaction outputs count
        /// </summary>
        private static readonly Int32 MAX_OUTPUTS = 4;

        /// <summary>
        /// List of transaction input UTXOs
        /// </summary>
        private UTXOData[] inputs = new UTXOData[MAX_INPUTS];

        /// <summary>
        /// List of transaction output data
        /// </summary>
        private TransactionOutputData[] outputs = new TransactionOutputData[MAX_OUTPUTS];

        /// <summary>
        /// List of signatures
        /// </summary>
        private string[] signatures = new string[MAX_INPUTS];

        /// <summary>
        /// Number of nonempty inputs
        /// </summary>
        private UInt32 inputCount = 0;

        /// <summary>
        /// Number of nonempty outputs
        /// </summary>
        private UInt32 outputCount = 0;

        /// <summary>
        /// Constructs empty transaction
        /// </summary>
        public Transaction()
        {
            for (Int32 i = 0; i < inputs.Length; ++i)
            {
                inputs[i] = UTXOData.Empty;
                signatures[i] = string.Empty;
            }

            // FIXME for now there are only fungible currencies on plasma, fix it later
            for (Int32 i = 0; i < outputs.Length; ++i)
            {
                outputs[i] = FCTransactionOutputData.Empty;
            }
        }

        /// <summary>
        /// Adds input to transaction
        /// </summary>
        /// <param name="data">transaction UTXO input data</param>
        public bool AddInput(UTXOData data)
        {
            if (inputCount <= MAX_INPUTS)
            {
                inputs[inputCount] = data;
                inputCount++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds output to transaction
        /// </summary>
        /// <param name="data">transaction output data</param>
        public bool AddOutput(TransactionOutputData data)
        {
            if (outputCount <= MAX_OUTPUTS)
            {
                outputs[outputCount] = data;
                outputCount++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds signature to transaction
        /// </summary>
        /// <param name="address"></param>
        /// <param name="signature"></param>
        public void AddSignature(string address, string signature)
        {
            for (Int32 i = 0; i < inputs.Length; ++i)
            {
                if (inputs[i].Owner == address)
                    signatures[i] = signature;
            }
        }

        /// <summary>
        /// Build RLP encoded transaction with signature
        /// </summary>
        /// <returns>RLP encoded signed transaction</returns>
        public byte[] GetRLPEncoded()
        {
            var rlpcollection = new List<byte[]>();

            var rlpSignatures = new List<byte[]>();
            for (Int32 i = 0; i < inputCount; ++i)
            {
                if(signatures[i] == string.Empty)
                {
                    throw new ArgumentNullException(string.Format("Missing signature for {0}", inputs[i].Owner));
                }
                rlpSignatures.Add(RLP.EncodeElement(signatures[i].HexToByteArray()));
            }

            rlpcollection.Add(RLP.EncodeList(rlpSignatures.ToArray()));

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

        /// <summary>
        /// Build RLP encoded transaction without signature
        /// </summary>
        /// <returns>RLP encoded transaction</returns>
        public byte[] GetRLPEncodedRaw()
        {
            var rlpcollection = new List<byte[]>();

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
