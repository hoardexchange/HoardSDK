using PlasmaCore.RPC.OutputData;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Status
{
    /// <summary>
    /// RPC request handler - requests for supported token types (only available with plasma supporting NFT)
    /// </summary>
    public class GetTokenTypes : RPCRequestHandlerBase
    {
        static private string route = "status.token_types";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public GetTokenTypes(IClient client) : base(client) { }

        /// <summary>
        /// Returns token type data of the child chain
        /// </summary>
        /// <returns></returns>
        public async Task<TokenTypeData[]> SendRequestAsync()
        {
            RPCRequest request = new RPCRequest(route);
            return await client.SendRequestAsync<TokenTypeData[]>(request);
        }
    }
}
