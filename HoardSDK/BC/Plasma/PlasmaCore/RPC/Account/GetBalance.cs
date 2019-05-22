using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Account
{
    public class GetBalance
    {
        static private string route = "account.get_balance";

        private IClient client;

        public GetBalance(IClient _client)
        {
            client = _client;
        }

        public async Task<BalanceData[]> SendRequestAsync(string address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            RpcRequest request = new RpcRequest(route);
            request.Parameters.Add("address", address.EnsureHexPrefix());

            return await client.SendRequestAsync<BalanceData[]>(request);
        }
    }
}
