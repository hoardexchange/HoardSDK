namespace PlasmaCore.Transactions
{
    /// <summary>
    /// Interface of transaction encoding (signing differs between version 0.1 (ari) and 0.2 (samrong)
    /// </summary>
    public interface ITransactionEncoder
    {
        /// <summary>
        /// Build raw transaction data ready for signing
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>encoded transaction without signatures</returns>
        byte[] EncodeRaw(Transaction transaction);

        /// <summary>
        /// Build encoded signed transaction data
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>encoded transaction with signatures</returns>
        byte[] EncodeSigned(Transaction transaction);

        /// <summary>
        /// Creates transaction from rlp encoded bytes data
        /// </summary>
        /// <param name="rlpEncodedTrasaction">rlp encoded bytes transaction data</param>
        /// <returns></returns>
        Transaction CreateTransaction(byte[] rlpEncodedTrasaction);
    }
}
