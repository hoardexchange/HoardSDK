using System.Collections.Generic;
using System.Numerics;

namespace PlasmaCore.Transaction
{
    /// <summary>
    /// Description of plasma transaction output data
    /// </summary>
    public abstract class TransactionOutputData
    {
        private static readonly string ZERO_ADDRESS = "0x0000000000000000000000000000000000000000";

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
        public abstract List<byte[]> GetRLPEncoded();
    }
}
