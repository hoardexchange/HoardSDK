using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Transaction
{
    /// <summary>
    /// RPC request handler - fetches all transaction data
    /// </summary>
    public class GetAllTransactions : RPCRequestHandlerBase
    {
        static private string route = "transaction.all";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public GetAllTransactions(IClient client) : base(client) { }

        /// <summary>
        /// Returns all transaction data filtered by optional parameters
        /// </summary>
        /// <param name="address">account address (optional)</param>
        /// <param name="blknum">block number (optional)</param>
        /// <param name="limit">result limit (optional)</param>
        /// <returns></returns>
        public async Task<TransactionsData[]> SendRequestAsync(string address = null, UInt64? blknum = null, UInt32? limit = null)
        {
            RPCRequest request = new RPCRequest(route);

            if (address != null)
                request.Parameters.Add("address", address.EnsureHexPrefix());

            if (blknum != null)
                request.Parameters.Add("blknum", blknum);

            if (limit != null)
                request.Parameters.Add("limit", limit);

            return await client.SendRequestAsync<TransactionsData[]>(request);
        }
    }
}
