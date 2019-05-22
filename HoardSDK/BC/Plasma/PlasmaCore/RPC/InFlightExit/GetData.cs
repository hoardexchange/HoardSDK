using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.InFlightExit
{
    public class GetData
    {
        static private string route = "in_flight_exit.get_data";

        private IClient client;

        public GetData(IClient _client)
        {
            client = _client;
        }

        public async Task<InFlightExitData> SendRequestAsync(string txBytes)
        {
            if (txBytes == null) throw new ArgumentNullException(nameof(txBytes));

            RpcRequest request = new RpcRequest(route);
            request.Parameters.Add("txBytes", txBytes.EnsureHexPrefix());

            return await client.SendRequestAsync<InFlightExitData>(request);
        }
    }
}
