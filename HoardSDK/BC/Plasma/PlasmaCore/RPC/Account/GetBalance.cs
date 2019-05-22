using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Account
{
    /// <summary>
    /// RPC request handler - fetches balance data of given address
    /// </summary>
    public class GetBalance : RPCRequestHandlerBase
    {
        static private string route = "account.get_balance";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public GetBalance(IClient client) : base(client) { }

        /// <summary>
        /// Returns balance data of given address
        /// </summary>
        /// <param name="address">account address</param>
        /// <returns></returns>
        public async Task<BalanceData[]> SendRequestAsync(string address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            RPCRequest request = new RPCRequest(route);
            request.Parameters.Add("address", address.EnsureHexPrefix());

            return await client.SendRequestAsync<BalanceData[]>(request);
        }
    }
}
