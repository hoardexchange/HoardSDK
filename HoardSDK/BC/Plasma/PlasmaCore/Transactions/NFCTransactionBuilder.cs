using PlasmaCore.UTXO;

namespace PlasmaCore.Transactions
{
    /// <summary>
    /// Plasma transaction builder helper class (non fungible currencies)
    /// </summary>
    public static class NFCTransactionBuilder
    {
        /// <summary>
        /// Builds the simplest, the most common transaction (one non fungible currency, one receiver, one sender, one token)
        /// </summary>
        /// <param name="addreesFrom">account of owner</param>
        /// <param name="addressTo">address of destination account</param>
        /// <param name="utxo">input utxo with given token id</param>
        /// <param name="currency">currency of transfer</param>
        /// <returns></returns>
        public static Transaction Build(string addreesFrom, string addressTo, UTXOData utxo, string currency)
        {
            if (utxo.Currency == currency)
            {
                var tx = new Transaction();
                tx.AddInput(utxo);
                tx.AddOutput(addressTo, currency, utxo.Data);
                return tx;
            }

            return null;
        }
    }
}
