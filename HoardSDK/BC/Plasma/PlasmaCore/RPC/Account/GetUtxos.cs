using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.UTXO;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Account
{
    /// <summary>
    /// RPC request handler - fetches utxo data of given address
    /// </summary>
    public class GetUtxos : RPCRequestHandlerBase
    {
        static private string route = "account.get_utxos";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public GetUtxos(IClient client) : base(client) { }

        /// <summary>
        /// Returns utxo data of given address
        /// </summary>
        /// <param name="address">account address</param>
        /// <returns></returns>
        public Task<UTXOData[]> SendRequestAsync(string address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            RPCRequest request = new RPCRequest(route);
            request.Parameters.Add("address", address.EnsureHexPrefix());

            return client.SendRequestAsync<UTXOData[]>(request);
        }
    }
}
