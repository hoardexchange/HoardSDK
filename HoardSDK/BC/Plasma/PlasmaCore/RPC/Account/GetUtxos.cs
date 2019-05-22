using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.UTXO;
using System;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.Account
{
    public class GetUtxos
    {
        static private string route = "account.get_utxos";

        private IClient client;

        public GetUtxos(IClient _client)
        {
            client = _client;
        }

        public Task<UTXOData[]> SendRequestAsync(string address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            RpcRequest request = new RpcRequest(route);
            request.Parameters.Add("address", address.EnsureHexPrefix());

            return client.SendRequestAsync<UTXOData[]>(request);
        }
    }
}
