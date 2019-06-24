using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlasmaCore.Transactions
{
    /// <summary>
    /// Encoder of raw transactions (not typed)
    /// </summary>
    public class RawTransactionEncoder : ITransactionEncoder
    {
        /// <summary>
        /// Maximum transaction inputs count
        /// </summary>
        private static readonly Int32 MAX_INPUTS = 4;

        /// <summary>
        /// Maximum transaction outputs count
        /// </summary>
        private static readonly Int32 MAX_OUTPUTS = 4;

        /// <inheritdoc/>
        public byte[] EncodeRaw(Transaction transaction)
        {
            var rlpcollection = new List<byte[]>();

            var rlpInputs = new List<byte[]>();
            transaction.Inputs.ForEach(x => rlpInputs.Add(x.GetRLPEncoded()));
            rlpInputs.AddRange(Enumerable.Repeat(new TransactionInputData().GetRLPEncoded(), MAX_INPUTS - transaction.Inputs.Count));
            rlpcollection.Add(RLP.EncodeList(rlpInputs.ToArray()));

            var rlpOutputs = new List<byte[]>();
            transaction.Outputs.ForEach(x => rlpOutputs.Add(x.GetRLPEncoded()));
            rlpOutputs.AddRange(Enumerable.Repeat(new TransactionOutputData().GetRLPEncoded(), MAX_OUTPUTS - transaction.Outputs.Count));
            rlpcollection.Add(RLP.EncodeList(rlpOutputs.ToArray()));

            return RLP.EncodeList(rlpcollection.ToArray());
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

            return RLP.EncodeList(rlpcollection.ToArray());
        }

        /// <inheritdoc/>
        public Transaction CreateTransaction(byte[] rlpEncodedTrasaction)
        {
            Transaction transaction = new Transaction();

            RLPCollection decodedList = (RLPCollection)RLP.Decode(rlpEncodedTrasaction)[0];

            bool isSigned = (decodedList.Count == 3);
            int inputIdx = isSigned ? 1 : 0;
            int outputIdx = isSigned ? 2 : 1;

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
    }
}
