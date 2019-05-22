using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Transaction
{
    public class GetAllTransactions
    {
        static private string route = "transaction.all";

        private IClient client;

        public GetAllTransactions(IClient _client)
        {
            client = _client;
        }

        public async Task<TransactionsData[]> SendRequestAsync(string address, UInt64 blknum, UInt32 limit)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            RpcRequest request = new RpcRequest(route);
            request.Parameters.Add("address", address.EnsureHexPrefix());
            request.Parameters.Add("blknum", blknum);
            request.Parameters.Add("limit", limit);

            return await client.SendRequestAsync<TransactionsData[]>(request);
        }
    }
}
