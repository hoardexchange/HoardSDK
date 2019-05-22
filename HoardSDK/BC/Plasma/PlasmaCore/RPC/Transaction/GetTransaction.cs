using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Transaction
{
    /// <summary>
    /// RPC request handler - fetches transaction data with given hash
    /// </summary>
    public class GetTransaction : RPCRequestHandlerBase
    {
        static private string route = "transaction.get";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public GetTransaction(IClient client) : base(client) { }

        /// <summary>
        /// Returns transaction data with given hash
        /// </summary>
        /// <param name="txHash">transaction hash (id)</param>
        /// <returns></returns>
        public async Task<TransactionDetails> SendRequestAsync(string txHash)
        {
            if (txHash == null) throw new ArgumentNullException(nameof(txHash));

            RPCRequest request = new RPCRequest(route);
            request.Parameters.Add("id", txHash.EnsureHexPrefix());

            return await client.SendRequestAsync<TransactionDetails>(request);
        }
    }
}
