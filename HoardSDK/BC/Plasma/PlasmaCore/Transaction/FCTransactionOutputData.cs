using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using System.Collections.Generic;
using System.Numerics;

namespace PlasmaCore.Transaction
{
    /// <summary>
    /// Description of Plasma fungible currency transaction output data (ether, ERC20)
    /// </summary>
    public class FCTransactionOutputData : TransactionOutputData
    {
        private static readonly string ZERO_ADDRESS = "0x0000000000000000000000000000000000000000";

        /// <summary>
        /// Amount of the transaction
        /// </summary>
        public BigInteger Amount { get; protected set; }

        /// <summary>
        /// Creates fungible transaction output data
        /// </summary>
        /// <param name="owner">transaction receiver</param>
        /// <param name="currency">transaction currency</param>
        /// <param name="amount">transaction amount</param>
        public FCTransactionOutputData(string owner, string currency, BigInteger amount)
            : base(owner, currency)
        {
            Amount = amount;
        }

        /// <inheritdoc/>
        public override List<byte[]> GetRLPEncoded()
        {
            var data = new List<byte[]>();
            data.Add(RLP.EncodeElement(Owner.HexToByteArray()));
            data.Add(RLP.EncodeElement(Currency.HexToByteArray()));
            data.Add(RLP.EncodeElement(Amount.ToBytesForRLPEncoding()));
            return data;
        }

        /// <summary>
        /// Returns empty transaction output data
        /// </summary>
        public static FCTransactionOutputData Empty
        {
            get
            {
                return new FCTransactionOutputData(
                    ZERO_ADDRESS,
                    ZERO_ADDRESS,
                    BigInteger.Zero
                );
            }
        }
    }
}
