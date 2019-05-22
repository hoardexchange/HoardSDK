using Newtonsoft.Json.Linq;

namespace PlasmaCore.RPC
{
    public class RpcRequest
    {
        public string Route { get; protected set; }
        public JObject Parameters { get; protected set; }

        public RpcRequest(string method)
        {
            Route = method;
            Parameters = new JObject();
        }
    }
}
