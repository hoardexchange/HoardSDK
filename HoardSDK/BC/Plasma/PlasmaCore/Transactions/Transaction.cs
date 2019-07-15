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
        public List<TransactionInputData> Inputs { get; protected set; } = new List<TransactionInputData>();

        /// <summary>
        /// List of transaction output data
        /// </summary>
        public List<TransactionOutputData> Outputs { get; protected set; }  = new List<TransactionOutputData>();

        /// <summary>
        /// Transaction metadata (optional)
        /// </summary>
        public byte[] Metadata { get; protected set; } = "0x0000000000000000000000000000000000000000000000000000000000000000".HexToByteArray();

        private byte[][] signatures = new byte[0][];
        /// <summary>
        /// List of signatures
        /// </summary>
        public byte[][] Signatures { get { return signatures; } }

        private string[] senders = new string[0];
        /// <summary>
        /// List of input utxo owners
        /// </summary>
        public string[] Senders { get { return senders; } }

        /// <summary>
        /// Constructs empty transaction
        /// </summary>
        public Transaction() { }

        /// <summary>
        /// Adds input to transaction
        /// </summary>
        /// <param name="data">transaction utxo input data</param>
        public bool AddInput(UTXOData data)
        {
            if (Inputs.Count <= MAX_INPUTS)
            {
                var tid = new TransactionInputData(data.BlkNum, data.TxIndex, data.OIndex);
                if (!tid.IsEmpty())
                {
                    Inputs.Add(tid);

                    if (senders.Length < Inputs.Count)
                        Array.Resize(ref senders, Inputs.Count);
                    senders[Inputs.Count - 1] = data.Owner;

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds input to transaction
        /// </summary>
        /// <param name="blkNum">transaction block number</param>
        /// <param name="txIndex">transaction index</param>
        /// <param name="oIndex">transaction output index</param>
        public bool AddInput(ulong blkNum, UInt16 txIndex, UInt16 oIndex)
        {
            if (Inputs.Count <= MAX_INPUTS)
            {
                var tid = new TransactionInputData(blkNum, txIndex, oIndex);
                if (!tid.IsEmpty())
                {
                    Inputs.Add(tid);
                    if (senders.Length < Inputs.Count)
                        Array.Resize(ref senders, Inputs.Count);
                    senders[Inputs.Count - 1] = null;
                    return true;
                }
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
            if (Outputs.Count <= MAX_OUTPUTS)
            {
                var tod = new TransactionOutputData(owner, currency, amount.ToBytesForRLPEncoding());
                if (!tod.IsEmpty())
                {
                    Outputs.Add(tod);
                    return true;
                }
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
            if (signatures.Length < Inputs.Count)
                Array.Resize(ref signatures, Inputs.Count);
            if (senders.Length < Inputs.Count)
                Array.Resize(ref senders, Inputs.Count);

            bool found = false;
            for (Int32 i = 0; i < Inputs.Count; ++i)
            {
                if ((senders[i] != null) && (senders[i].EnsureHexPrefix().ToLower() == address.EnsureHexPrefix().ToLower()))
                {
                    signatures[i] = signature;
                    found = true;
                }
            }
            return found;
        }

        /// <summary>
        /// Sets signature of transaction for all inputs
        /// </summary>
        /// <param name="signature">sender signature</param>
        public void SetSignature(byte[] signature)
        {
            if (signatures.Length < Inputs.Count)
                Array.Resize(ref signatures, Inputs.Count);
            if (senders.Length < Inputs.Count)
                Array.Resize(ref senders, Inputs.Count);

            for (Int32 i = 0; i < Inputs.Count; ++i)
            {
                signatures[i] = signature;
            }
        }

        /// <summary>
        /// Sets signature to transaction by index
        /// </summary>
        /// <param name="idx">index of signature in transaction</param>
        /// <param name="signature">sender signature</param>
        public bool SetSignature(Int32 idx, byte[] signature)
        {
            if (signatures.Length < Inputs.Count)
                Array.Resize(ref signatures, Inputs.Count);
            if (senders.Length < Inputs.Count)
                Array.Resize(ref senders, Inputs.Count);

            if (signatures.Length > idx)
            {
                signatures[idx] = signature;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets transaction metadata
        /// </summary>
        /// <param name="data">metadata of transaction (max 32 bytes)</param>
        /// <returns></returns>
        public bool SetMetadata(byte[] data)
        {
            if(data.Length <= 32)
            {
                Metadata = data;
                return true;
            }
            return false;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ulong ToUInt64FromRLPDecoded(byte[] bytes)
        {
            if (bytes != null)
                return (ulong)(bytes.ToBigIntegerFromRLPDecoded());
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static UInt16 ToUInt16FromRLPDecoded(byte[] bytes)
        {
            if (bytes != null)
                return (UInt16)(bytes.ToBigIntegerFromRLPDecoded());
            return (UInt16)0;
        }
    }
}
