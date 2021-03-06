﻿using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using System.Numerics;

namespace PlasmaCore.Transactions
{
    /// <summary>
    /// Description of plasma transaction output data
    /// </summary>
    public class TransactionOutputData
    {
        /// <summary>
        /// Receiver of the transaction output
        /// </summary>
        public string Owner { get; protected set; }

        /// <summary>
        /// Currency of the transaction output
        /// </summary>
        public string Currency { get; protected set; }

        /// <summary>
        /// Value of the transaction output
        /// </summary>
        public byte[] Value { get; protected set; }

        /// <summary>
        /// Creates empty transaction output data
        /// </summary>
        public TransactionOutputData()
        {
            Owner = string.Empty.PadLeft(40, '0').EnsureHexPrefix();
            Currency = string.Empty.PadLeft(40, '0').EnsureHexPrefix();
            Value = BigInteger.Zero.ToBytesForRLPEncoding();
        }

        /// <summary>
        /// Creates base transaction output data
        /// </summary>
        /// <param name="owner">transaction receiver</param>
        /// <param name="currency">transaction currency</param>
        /// <param name="value"></param>
        public TransactionOutputData(string owner, string currency, byte[] value)
        {
            Owner = owner.EnsureHexPrefix();
            Currency = currency.EnsureHexPrefix();
            Value = value;
        }

        /// <summary>
        /// Returns rlp encoded transaction output data
        /// </summary>
        /// <returns></returns>
        public byte[] GetRLPEncoded()
        {
            var data = new byte[3][];
            data[0] = RLP.EncodeElement(Owner.HexToByteArray());
            data[1] = RLP.EncodeElement(Currency.HexToByteArray());
            data[2] = RLP.EncodeElement(Value);
            return RLP.EncodeList(data);
        }

        /// <summary>
        /// Returns if transaction output data is empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return (Owner == string.Empty.PadLeft(40, '0').EnsureHexPrefix() &&
                Currency == string.Empty.PadLeft(40, '0').EnsureHexPrefix() &&
                Value == BigInteger.Zero.ToBytesForRLPEncoding());
        }
    }
}
