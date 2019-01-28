using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Hoard.BC.Plasma
{
    /// <summary>
    /// Description of Plasma transaction output data
    /// </summary>
    public abstract class TransactionOutputData
    {
        /// <summary>
        /// Receiver of the transaction
        /// </summary>
        public string Owner { get; protected set; }

        /// <summary>
        /// Currency of the transaction
        /// </summary>
        public string Currency { get; protected set; }

        /// <summary>
        /// Creates base transaction output data
        /// </summary>
        /// <param name="owner">transaction receiver</param>
        /// <param name="currency">transaction currency</param>
        protected TransactionOutputData(string owner, string currency)
        {
            Owner = owner;
            Currency = currency;
        }

        /// <summary>
        /// Returns data required by transaction
        /// </summary>
        /// <returns></returns>
        public abstract List<byte[]> GetTxBytes();
    }

    /// <summary>
    /// Description of Plasma ERC223 transaction output data
    /// </summary>
    public class ERC223TransactionOutputData : TransactionOutputData
    {
        /// <summary>
        /// 
        /// </summary>
        public BigInteger Amount { get; protected set; }

        /// <summary>
        /// Gets empty ERC223 output
        /// </summary>
        public static TransactionOutputData Empty
        {
            get
            {
                return new ERC223TransactionOutputData(
                    PlasmaComm.ZERO_ADDRESS, 
                    PlasmaComm.ZERO_ADDRESS, 
                    BigInteger.Zero
                );
            }
        }

        /// <summary>
        /// Creates ERC223 transaction output data
        /// </summary>
        /// <param name="owner">transaction receiver</param>
        /// <param name="currency">transaction currency</param>
        /// <param name="amount">transaction amount</param>
        public ERC223TransactionOutputData(string owner, string currency, BigInteger amount)
            : base(owner, currency)
        {
            Amount = amount;
        }

        /// <inheritdoc/>
        public override List<byte[]> GetTxBytes()
        {
            var data = new List<byte[]>();
            data.Add(Owner.HexToByteArray());
            data.Add(Currency.HexToByteArray());
            data.Add(Amount.ToBytesForRLPEncoding());
            return data;
        }
    }

    /// <summary>
    /// Description of Plasma ERC721 transaction output data
    /// </summary>
    public class ERC721TransactionOutputData : TransactionOutputData
    {
        /// <summary>
        /// List of token ids being sent
        /// </summary>
        public List<BigInteger> TokenIds { get; protected set; }

        /// <summary>
        /// Gets empty ERC721 output
        /// </summary>
        public static TransactionOutputData Empty
        {
            get
            {
                return new ERC721TransactionOutputData(
                    PlasmaComm.ZERO_ADDRESS, 
                    PlasmaComm.ZERO_ADDRESS, 
                    new List<BigInteger>() { BigInteger.Zero }
                );
            }
        }

        /// <summary>
        /// Creates ERC721 transaction output data
        /// </summary>
        /// <param name="owner">transaction receiver</param>
        /// <param name="currency">transaction currency</param>
        /// <param name="tokenIds">list of token ids being sent</param>
        public ERC721TransactionOutputData(string owner, string currency, List<BigInteger> tokenIds)
            : base(owner, currency)
        {
            TokenIds = tokenIds;
        }

        /// <inheritdoc/>
        public override List<byte[]> GetTxBytes()
        {
            throw new NotImplementedException();
        }
    }
}
