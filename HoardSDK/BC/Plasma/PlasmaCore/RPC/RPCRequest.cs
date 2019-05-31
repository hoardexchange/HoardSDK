using Newtonsoft.Json.Linq;

namespace PlasmaCore.RPC
{
    /// <summary>
    /// RPC request data
    /// </summary>
    public class RPCRequest
    {
        /// <summary>
        /// Request route/path
        /// </summary>
        public string Route { get; protected set; }

        /// <summary>
        /// Reuqest parameters serialized into json object
        /// </summary>
        public JObject Parameters { get; protected set; }

        /// <summary>
        /// Creates RPC request with given route and empty paramaters
        /// </summary>
        /// <param name="route">request route</param>
        public RPCRequest(string route)
        {
            Route = route;
            Parameters = new JObject();
        }

        /// <summary>
        /// Creates RPC request with given route and paramaters
        /// </summary>
        /// <param name="route">request route</param>
        /// <param name="parameters">request parameters as json</param>
        public RPCRequest(string route, JObject parameters)
        {
            Route = route;
            Parameters = parameters;
        }
    }
}
