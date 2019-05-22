using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Transaction
{
    public class SubmitTransaction
    {
        static private string route = "transaction.submit";

        private IClient client;

        public SubmitTransaction(IClient _client)
        {
            client = _client;
        }

        public async Task<TransactionReceipt> SendRequestAsync(string transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            RpcRequest request = new RpcRequest(route);
            request.Parameters.Add("transaction", transaction.EnsureHexPrefix());

            return await client.SendRequestAsync<TransactionReceipt>(request);
        }
    }
}
