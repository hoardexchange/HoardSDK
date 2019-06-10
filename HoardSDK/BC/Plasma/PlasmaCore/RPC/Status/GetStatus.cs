using PlasmaCore.RPC.OutputData;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Status
{
    /// <summary>
    /// RPC request handler - monitors the ChildChain and report dishonest activity
    /// </summary>
    public class GetStatus : RPCRequestHandlerBase
    {
        static private string route = "status.get";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public GetStatus(IClient client) : base(client) { }

        /// <summary>
        /// Returns status data of the child chain
        /// </summary>
        /// <returns></returns>
        public async Task<StatusData> SendRequestAsync()
        {
            RPCRequest request = new RPCRequest(route);
            return await client.SendRequestAsync<StatusData>(request);
        }
    }
}
