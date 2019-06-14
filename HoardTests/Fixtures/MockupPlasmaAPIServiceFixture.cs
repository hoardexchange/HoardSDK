using Hoard.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PlasmaCore;
using PlasmaCore.RPC;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HoardTests.Fixtures
{
    public class MockupPlasmaAPIServiceFixture
    {
        public class MockupRPCClient : IClient
        {
            public Task<T> SendRequestAsync<T>(RPCRequest request)
            {
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                string responseMessage = "";
                if (request.Route == "account.get_utxos")
                {
                    responseMessage = PlasmaMockupResponses.GetUtxos(request.Parameters.Value<string>("address"));
                }
                else if (request.Route == "account.get_balance")
                {
                    responseMessage = PlasmaMockupResponses.GetBalance(request.Parameters.Value<string>("address"));
                }
                else if (request.Route == "transaction.submit")
                {
                    responseMessage = PlasmaMockupResponses.SubmitTransaction(request.Parameters.Value<string>("transaction"));
                }
                else if (request.Route == "utxo.get_challenge_data")
                {
                    responseMessage = PlasmaMockupResponses.GetChallengeData();
                }

                using (var reader = new JsonTextReader(new StringReader(responseMessage)))
                {
                    var serializer = JsonSerializer.Create(jsonSerializerSettings);
                    var message = serializer.Deserialize<RPCResponse>(reader);

                    return Task.FromResult(message.GetData<T>());
                }
            }

        }

        public PlasmaAPIService PlasmaAPIService { get; private set; }

        public MockupPlasmaAPIServiceFixture()
        {
            PlasmaAPIService = new PlasmaAPIService(new MockupRPCClient(), new MockupRPCClient());
        }
    }
}
