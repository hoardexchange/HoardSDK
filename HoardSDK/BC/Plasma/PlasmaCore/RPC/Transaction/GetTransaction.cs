using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Transaction
{
    public class GetTransaction
    {
        static private string route = "transaction.get";

        private IClient client;

        public GetTransaction(IClient _client)
        {
            client = _client;
        }

        public async Task<TransactionDetails> SendRequestAsync(string txHash)
        {
            if (txHash == null) throw new ArgumentNullException(nameof(txHash));

            RpcRequest request = new RpcRequest(route);
            request.Parameters.Add("id", txHash.EnsureHexPrefix());

            return await client.SendRequestAsync<TransactionDetails>(request);
        }
    }
}
