using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Account
{
    /// <summary>
    /// RPC request handler - fetches transaction data of given address
    /// </summary>
    public class GetTransactions : RPCRequestHandlerBase
    {
        static private string route = "account.get_transactions";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public GetTransactions(IClient client) : base(client) { }

        /// <summary>
        /// Returns transactions data of given address
        /// </summary>
        /// <param name="address">account address</param>
        /// <param name="limit">result limit</param>
        /// <returns></returns>
        public async Task<TransactionsData[]> SendRequestAsync(string address, uint limit)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            RPCRequest request = new RPCRequest(route);
            request.Parameters.Add("address", address.EnsureHexPrefix());
            request.Parameters.Add("limit", limit);

            return await client.SendRequestAsync<TransactionsData[]>(request);
        }
    }
}
