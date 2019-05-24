using Newtonsoft.Json;
using System.Numerics;

namespace PlasmaCore.UTXO
{
    /// <summary>
    /// Response class for unspent transaction output request (fungible currencies - ether, ERC20)
    /// </summary>
    public class FCUTXOData : UTXOData
    {
        /// <summary>
        /// Amount of tokens
        /// </summary>
        [JsonProperty(propertyName: "amount")]
        public BigInteger Amount { get; set; }
    }
}
