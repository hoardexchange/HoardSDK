using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.InFlightExit
{
    /// <summary>
    /// RPC request handler - fetches a competitor to an in-flight exit
    /// </summary>
    public class GetCompetitor : RPCRequestHandlerBase
    {
        static private string route = "in_flight_exit.get_competitor";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public GetCompetitor(IClient client) : base(client) { }

        /// <summary>
        /// Returns a competitor to an in-flight exit
        /// </summary>
        /// <param name="txBytes">in-flight transaction bytes body</param>
        /// <returns></returns>
        public async Task<CompetitorData> SendRequestAsync(string txBytes)
        {
            if (txBytes == null) throw new ArgumentNullException(nameof(txBytes));

            RPCRequest request = new RPCRequest(route);
            request.Parameters.Add("txBytes", txBytes.EnsureHexPrefix());

            return await client.SendRequestAsync<CompetitorData>(request);
        }
    }
}
