using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using PlasmaCore.EIP712;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PlasmaCore.Transactions
{
    /// <summary>
    /// Encodes child chain (v0.2 - Samrong) transaction using EIP-712 (Ethereum typed structured data hashing and signing standard)
    /// </summary>
    public class TypedDataTransactionEncoder : ITransactionEncoder
    {
        private static readonly Int32 MAX_INPUTS = 4;

        private static readonly Int32 MAX_OUTPUTS = 4;

        private EIP712Domain defaultDomain = new EIP712Domain(
            "OMG Network", 
            "1",
            "0x740ecec4c0ee99c285945de8b44e9f5bfb71eea7", 
            "0xfad5c7f626d80f9256ef01929f3beb96e058b8b4b0e3fe52d84f054c0e2a7a83".HexToByteArray());

        /// <summary>
        /// Constructs typed data transaction encoder with given domain
        /// </summary>
        /// <param name="domain">EIP-712 domain (optional, OMG Network is default)</param>
        public TypedDataTransactionEncoder(EIP712Domain domain = null)
        {
            if(domain != null)
                defaultDomain = domain;
        }

        /// <inheritdoc/>
        public byte[] EncodeRaw(Transaction transaction)
        {
            PlasmaCore.EIP712.Transaction eip712Tx = new PlasmaCore.EIP712.Transaction();
            eip712Tx.Input0 = transaction.Inputs.Count > 0 ? CreateEIP712Input(transaction.Inputs[0]) : CreateEIP712Input(new TransactionInputData());
            eip712Tx.Input1 = transaction.Inputs.Count > 1 ? CreateEIP712Input(transaction.Inputs[1]) : CreateEIP712Input(new TransactionInputData());
            eip712Tx.Input2 = transaction.Inputs.Count > 2 ? CreateEIP712Input(transaction.Inputs[2]) : CreateEIP712Input(new TransactionInputData());
            eip712Tx.Input3 = transaction.Inputs.Count > 3 ? CreateEIP712Input(transaction.Inputs[3]) : CreateEIP712Input(new TransactionInputData());

            eip712Tx.Output0 = transaction.Outputs.Count > 0 ? CreateEIP712Output(transaction.Outputs[0]) : CreateEIP712Output(new TransactionOutputData());
            eip712Tx.Output1 = transaction.Outputs.Count > 1 ? CreateEIP712Output(transaction.Outputs[1]) : CreateEIP712Output(new TransactionOutputData());
            eip712Tx.Output2 = transaction.Outputs.Count > 2 ? CreateEIP712Output(transaction.Outputs[2]) : CreateEIP712Output(new TransactionOutputData());
            eip712Tx.Output3 = transaction.Outputs.Count > 3 ? CreateEIP712Output(transaction.Outputs[3]) : CreateEIP712Output(new TransactionOutputData());

            eip712Tx.Metadata = transaction.Metadata;

            return TypedDataEncoder.Encode(eip712Tx, defaultDomain);
        }

        /// <inheritdoc/>
        public byte[] EncodeSigned(Transaction transaction)
        {
            var rlpcollection = new List<byte[]>();

            var rlpSignatures = new List<byte[]>();
            for (Int32 i = 0; i < transaction.Inputs.Count; ++i)
            {
                if (transaction.Signatures[i] != null)
                {
                    rlpSignatures.Add(RLP.EncodeElement(transaction.Signatures[i]));
                }
                else
                {
                    // missing signature - cannot return valid encoded transaction
                    return new byte[0];
                }
            }

            rlpcollection.Add(RLP.EncodeList(rlpSignatures.ToArray()));

            var rlpInputs = new List<byte[]>();
            transaction.Inputs.ForEach(x => rlpInputs.Add(x.GetRLPEncoded()));
            rlpInputs.AddRange(Enumerable.Repeat(new TransactionInputData().GetRLPEncoded(), MAX_INPUTS - transaction.Inputs.Count));
            rlpcollection.Add(RLP.EncodeList(rlpInputs.ToArray()));

            var rlpOutputs = new List<byte[]>();
            transaction.Outputs.ForEach(x => rlpOutputs.Add(x.GetRLPEncoded()));
            rlpOutputs.AddRange(Enumerable.Repeat(new TransactionOutputData().GetRLPEncoded(), MAX_OUTPUTS - transaction.Outputs.Count));
            rlpcollection.Add(RLP.EncodeList(rlpOutputs.ToArray()));

            rlpcollection.Add(RLP.EncodeElement(transaction.Metadata));

            return RLP.EncodeList(rlpcollection.ToArray());
        }

        /// <inheritdoc/>
        public Transaction CreateTransaction(byte[] rlpEncodedTrasaction)
        {
            Transaction transaction = new Transaction();

            RLPCollection decodedList = (RLPCollection)RLP.Decode(rlpEncodedTrasaction)[0];

            bool isSigned = (decodedList.Count == 4);
            int inputIdx = isSigned ? 1 : 0;
            int outputIdx = isSigned ? 2 : 1;
            int metadataIdx = isSigned ? 3 : 2;

            RLPCollection inputData = (RLPCollection)decodedList[inputIdx];
            foreach (RLPCollection input in inputData)
            {
                if (input.Count == 3)
                {
                    transaction.AddInput(Transaction.ToUInt64FromRLPDecoded(input[0].RLPData),
                             Transaction.ToUInt16FromRLPDecoded(input[1].RLPData),
                             Transaction.ToUInt16FromRLPDecoded(input[2].RLPData));
                }
            }

            RLPCollection outputData = (RLPCollection)decodedList[outputIdx];
            foreach (RLPCollection output in outputData)
            {
                if (output.Count == 3)
                {
                    transaction.AddOutput(output[0].RLPData.ToHex().PadLeft(32, '0').EnsureHexPrefix(),
                              output[1].RLPData.ToHex().PadLeft(32, '0').EnsureHexPrefix(),
                              output[2].RLPData.ToBigIntegerFromRLPDecoded());
                }
            }

            RLPCollection metadata = (RLPCollection)decodedList[metadataIdx];
            transaction.SetMetadata(metadata.RLPData.ToHex().HexToByteArray());

            if (isSigned)
            {
                RLPCollection signatureData = (RLPCollection)decodedList[0];
                for (Int32 i = 0; i < signatureData.Count; ++i)
                {
                    transaction.SetSignature(i, signatureData[i].RLPData);
                }
            }

            return transaction;
        }

        private PlasmaCore.EIP712.Input CreateEIP712Input(TransactionInputData input)
        {
            return new PlasmaCore.EIP712.Input
            {
                BlkNum = input.BlkNum,
                TxIndex = input.TxIndex,
                OIndex = input.OIndex
            };
        }

        private PlasmaCore.EIP712.Output CreateEIP712Output(TransactionOutputData output)
        {
            return new PlasmaCore.EIP712.Output
            {
                Owner = output.Owner,
                Currency = output.Currency,
                Amount = output.Value.ToBigIntegerFromRLPDecoded()
            };
        }
    }
}

namespace PlasmaCore.EIP712
{
    [TypedStruct("Transaction")]
    internal class Transaction
    {
        [TypedData("input0", "Input")]
        public Input Input0 { get; set; }

        [TypedData("input1", "Input")]
        public Input Input1 { get; set; }

        [TypedData("input2", "Input")]
        public Input Input2 { get; set; }

        [TypedData("input3", "Input")]
        public Input Input3 { get; set; }

        [TypedData("output0", "Output")]
        public Output Output0 { get; set; }

        [TypedData("output1", "Output")]
        public Output Output1 { get; set; }

        [TypedData("output2", "Output")]
        public Output Output2 { get; set; }

        [TypedData("output3", "Output")]
        public Output Output3 { get; set; }

        [TypedData("metadata", "bytes32")]
        public byte[] Metadata { get; set; }
    }

    [TypedStruct("Input")]
    internal class Input
    {
        [TypedData("blknum", "uint256")]
        public BigInteger BlkNum { get; set; }

        [TypedData("txindex", "uint256")]
        public BigInteger TxIndex { get; set; }

        [TypedData("oindex", "uint256")]
        public BigInteger OIndex { get; set; }
    }

    [TypedStruct("Output")]
    internal class Output
    {
        [TypedData("owner", "address")]
        public string Owner { get; set; }

        [TypedData("currency", "address")]
        public string Currency { get; set; }

        [TypedData("amount", "uint256")]
        public BigInteger Amount { get; set; }
    }
}

