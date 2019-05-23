using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using PlasmaCore.UTXO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PlasmaCore.Transactions
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
        /// List of transaction input utxo data
        /// </summary>
        private TransactionInputData[] inputs = new TransactionInputData[MAX_INPUTS];

        /// <summary>
        /// List of transaction output data
        /// </summary>
        private TransactionOutputData[] outputs = new TransactionOutputData[MAX_OUTPUTS];

        /// <summary>
        /// List of signatures
        /// </summary>
        private byte[][] signatures = new byte[MAX_INPUTS][];
        private string[] senders = new string[MAX_INPUTS];

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
                inputs[i] = new TransactionInputData();
                signatures[i] = null;
                senders[i] = null;
            }

            // FIXME for now there are only fungible currencies on plasma, fix it later
            for (Int32 i = 0; i < outputs.Length; ++i)
            {
                outputs[i] = new TransactionOutputData();
            }
        }

        /// <summary>
        // Constructs transaction from rlp encoded transaction bytes
        /// </summary>
        /// <param name="rlpEncodedTrasaction">rlp encoded transaction bytes</param>
        public Transaction(byte[] rlpEncodedTrasaction)
        {
            //rlp decoding should look like this for the latest Nethereum.RLP library version 3.3.0 - update on Nethereum upgrade
            //RLPCollection decodedList = (RLPCollection)RLP.Decode(rlpEncodedTrasaction);

            RLPCollection decodedList = (RLPCollection)RLP.Decode(rlpEncodedTrasaction)[0];

            bool isSigned = (decodedList.Count == 3);
            int inputIdx = isSigned ? 1 : 0;
            int outputIdx = isSigned ? 2 : 1;

            RLPCollection inputData = (RLPCollection)decodedList[inputIdx];
            foreach (RLPCollection input in inputData)
            {
                if (input.Count == 3)
                {
                    AddInput(ToUInt64FromRLPDecoded(input[0].RLPData),
                             ToUInt16FromRLPDecoded(input[1].RLPData),
                             ToUInt16FromRLPDecoded(input[2].RLPData));
                }
            }

            RLPCollection outputData = (RLPCollection)decodedList[outputIdx];
            foreach (RLPCollection output in outputData)
            {
                if (output.Count == 3)
                {
                    AddOutput(output[0].RLPData.ToHex().PadLeft(32, '0').EnsureHexPrefix(),
                              output[1].RLPData.ToHex().PadLeft(32, '0').EnsureHexPrefix(),
                              output[2].RLPData.ToBigIntegerFromRLPDecoded());
                }
            }

            if(isSigned)
            {
                RLPCollection signatureData = (RLPCollection)decodedList[0];
                for(Int32 i = 0; i < signatureData.Count; ++i)
                {
                    SetSignature(i, signatureData[i].RLPData);
                }
            }
        }

        /// <summary>
        /// Adds input to transaction
        /// </summary>
        /// <param name="data">transaction utxo input data</param>
        public bool AddInput(UTXOData data)
        {
            if (inputCount <= MAX_INPUTS)
            {
                inputs[inputCount] = new TransactionInputData(data.BlkNum, data.TxIndex, data.OIndex);
                senders[inputCount] = data.Owner;
                inputCount++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds input to transaction
        /// </summary>
        /// <param name="blkNum">transaction block number</param>
        /// <param name="txIndex">transaction index</param>
        /// <param name="oIndex">transaction output index</param>
        public bool AddInput(UInt64 blkNum, UInt16 txIndex, UInt16 oIndex)
        {
            if (inputCount <= MAX_INPUTS)
            {
                inputs[inputCount] = new TransactionInputData(blkNum, txIndex, oIndex);
                inputCount++;
                return true;
            }
            return false;
        }


        /// <summary>
        /// Adds output to transaction
        /// </summary>
        /// <param name="owner">transaction output recipient</param>
        /// <param name="currency">transaction output currency</param>
        /// <param name="amount">transaction output amount</param>
        public bool AddOutput(string owner, string currency, BigInteger amount)
        {
            if (outputCount <= MAX_OUTPUTS)
            {
                outputs[outputCount] = new TransactionOutputData(owner, currency, amount.ToBytesForRLPEncoding());
                outputCount++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets signature of transaction by sender address
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="signature">sender signature</param>
        public bool SetSignature(string address, byte[] signature)
        {
            bool found = false;
            for (Int32 i = 0; i < inputs.Length; ++i)
            {
                if (senders[i].EnsureHexPrefix().ToLower() == address.EnsureHexPrefix().ToLower())
                {
                    signatures[i] = signature;
                    found = true;
                }
            }
            return found;
        }

        /// <summary>
        /// Sets signature to transaction by index
        /// </summary>
        /// <param name="idx">index of signature in transaction</param>
        /// <param name="signature">sender signature</param>
        public bool SetSignature(Int32 idx, byte[] signature)
        {
            if(signatures.Length > idx)
            {
                signatures[idx] = signature;
                return true;
            }
            return false;
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
                if (signatures[i] != null)
                {
                    rlpSignatures.Add(RLP.EncodeElement(signatures[i]));
                }
                else
                {
                    // missing signature - cannot return valid encoded transaction
                    return null;
                }
            }

            rlpcollection.Add(RLP.EncodeList(rlpSignatures.ToArray()));

            var rlpInputs = new List<byte[]>();
            Array.ForEach(inputs, x => rlpInputs.Add(x.GetRLPEncoded()));
            rlpcollection.Add(RLP.EncodeList(rlpInputs.ToArray()));

            var rlpOutputs = new List<byte[]>();
            Array.ForEach(outputs, x => rlpOutputs.Add(x.GetRLPEncoded()));
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
            Array.ForEach(inputs, x => rlpInputs.Add(x.GetRLPEncoded()));
            rlpcollection.Add(RLP.EncodeList(rlpInputs.ToArray()));

            var rlpOutputs = new List<byte[]>();
            Array.ForEach(outputs, x => rlpOutputs.Add(x.GetRLPEncoded()));
            rlpcollection.Add(RLP.EncodeList(rlpOutputs.ToArray()));

            return RLP.EncodeList(rlpcollection.ToArray());
        }

        /// <summary>
        /// Returns transaction senders. It will always return empty array for transactions decoded from rlp data.
        /// </summary>
        /// <returns></returns>
        public string[] GetSenders()
        {
            return senders.Where(x => x != null).Distinct().ToArray();
        }

        // RLP helpers

        public static UInt64 ToUInt64FromRLPDecoded(byte[] bytes)
        {
            if (bytes != null)
                return (UInt64)(bytes.ToBigIntegerFromRLPDecoded());
            return 0;
        }

        public static UInt16 ToUInt16FromRLPDecoded(byte[] bytes)
        {
            if (bytes != null)
                return (UInt16)(bytes.ToBigIntegerFromRLPDecoded());
            return (UInt16)0;
        }
    }
}
