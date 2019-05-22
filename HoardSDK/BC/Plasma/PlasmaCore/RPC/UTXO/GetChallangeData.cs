using Newtonsoft.Json.Linq;
using PlasmaCore.RPC.OutputData;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.UTXO
{
    public class GetChallangeData
    {
        static private string route = "utxo.get_challenge_data";

        private IClient client;

        public GetChallangeData(IClient _client)
        {
            client = _client;
        }

        public async Task<ChallengeData> SendRequestAsync(BigInteger position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));

            RpcRequest request = new RpcRequest(route);
            request.Parameters.Add("utxo_pos", new JValue(position));

            return await client.SendRequestAsync<ChallengeData>(request);
        }
    }
}
