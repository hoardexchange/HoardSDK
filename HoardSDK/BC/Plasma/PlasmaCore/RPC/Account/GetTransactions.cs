using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Account
{
    public class GetTransactions
    {
        static private string route = "account.get_transactions";

        private IClient client;

        public GetTransactions(IClient _client)
        {
            client = _client;
        }

        public async Task<TransactionsData[]> SendRequestAsync(string address, uint limit)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            RpcRequest request = new RpcRequest(route);
            request.Parameters.Add("address", address.EnsureHexPrefix());
            request.Parameters.Add("limit", limit);

            return await client.SendRequestAsync<TransactionsData[]>(request);
        }
    }
}
