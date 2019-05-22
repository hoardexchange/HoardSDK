using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Transaction
{
    /// <summary>
    /// RPC request handler - submits signed transaction to the child chain
    /// </summary>
    public class SubmitTransaction : RPCRequestHandlerBase
    {
        static private string route = "transaction.submit";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public SubmitTransaction(IClient client) : base(client) { }

        /// <summary>
        /// Submits signed transaction to the child chain and returns transaction receipt
        /// </summary>
        /// <param name="transaction">signed transaction</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> SendRequestAsync(string transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            RPCRequest request = new RPCRequest(route);
            request.Parameters.Add("transaction", transaction.EnsureHexPrefix());

            return await client.SendRequestAsync<TransactionReceipt>(request);
        }
    }
}
