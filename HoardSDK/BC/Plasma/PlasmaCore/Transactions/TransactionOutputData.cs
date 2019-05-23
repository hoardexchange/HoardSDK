using Nethereum.Hex.HexConvertors.Extensions;
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
        public byte[] RLPEncodedValue { get; protected set; }

        /// <summary>
        /// Creates empty transaction output data
        /// </summary>
        public TransactionOutputData()
        {
            Owner = string.Empty.PadLeft(40, '0').EnsureHexPrefix();
            Currency = string.Empty.PadLeft(40, '0').EnsureHexPrefix();
            RLPEncodedValue = RLP.EncodeElement(BigInteger.Zero.ToBytesForRLPEncoding());
        }

        /// <summary>
        /// Creates base transaction output data
        /// </summary>
        /// <param name="owner">transaction receiver</param>
        /// <param name="currency">transaction currency</param>
        /// <param name="value"></param>
        public TransactionOutputData(string owner, string currency, byte[] value)
        {
            Owner = owner;
            Currency = currency;
            RLPEncodedValue = RLP.EncodeElement(value);
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
            data[2] = RLPEncodedValue;
            return RLP.EncodeList(data);
        }
    }
}
