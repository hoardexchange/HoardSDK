using Newtonsoft.Json;
using System.Numerics;

namespace PlasmaCore.UTXO
{
    /// <summary>
    /// Description of Plasma fungible currency Unspent Transaction Output (ether, ERC20)
    /// </summary>
    public class FCUTXOData : UTXOData
    {
        private static BigInteger U256_MAX_VALUE = BigInteger.Pow(2, 256);

        /// <summary>
        /// Amount of tokens
        /// </summary>
        [JsonProperty(propertyName: "amount")]
        public BigInteger Amount { get; protected set; }
    }
}
