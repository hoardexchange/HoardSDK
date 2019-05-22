using Newtonsoft.Json.Linq;
using PlasmaCore.RPC.OutputData;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.UTXO
{
    /// <summary>
    /// RPC request handler - fetches challenge data for given utxo exit
    /// </summary>
    public class GetChallangeData : RPCRequestHandlerBase
    {
        static private string route = "utxo.get_challenge_data";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public GetChallangeData(IClient client) : base(client) { }

        /// <summary>
        /// Returns challenge data for given utxo exit
        /// </summary>
        /// <param name="position">utxo position</param>
        /// <returns></returns>
        public async Task<ChallengeData> SendRequestAsync(BigInteger position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));

            RPCRequest request = new RPCRequest(route);
            request.Parameters.Add("utxo_pos", new JValue(position));

            return await client.SendRequestAsync<ChallengeData>(request);
        }
    }
}
