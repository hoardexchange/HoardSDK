using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.InFlightExit
{
    /// <summary>
    /// RPC request handler - fetches exit data for an in-flight exit
    /// </summary>
    public class GetInFlightExit : RPCRequestHandlerBase
    {
        static private string route = "in_flight_exit.get_data";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public GetInFlightExit(IClient client) : base(client) { }

        /// <summary>
        /// Returns exit data for an in-flight exit
        /// </summary>
        /// <param name="txBytes">in-flight transaction bytes body</param>
        /// <returns></returns>
        public async Task<InFlightExitData> SendRequestAsync(string txBytes)
        {
            if (txBytes == null) throw new ArgumentNullException(nameof(txBytes));

            RPCRequest request = new RPCRequest(route);
            request.Parameters.Add("txBytes", txBytes.EnsureHexPrefix());

            return await client.SendRequestAsync<InFlightExitData>(request);
        }
    }
}
