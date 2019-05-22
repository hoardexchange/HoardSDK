using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Hoard
{
    internal enum BCClientType
    {
        Unknown,
        Ethereum,
        Plasma
    }

    internal class BCClientConfigConverter : JsonConverter
    {
        static BCClientConfigConverter()
        {
            new BCClientConfigConverter();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jObject = JToken.ReadFrom(reader);
            BCClientType type = jObject["Type"].ToObject<BCClientType>();

            BCClientConfig result;
            switch (type)
            {
                case BCClientType.Ethereum:
                    result = new EthereumClientConfig();
                    break;
                case BCClientType.Plasma:
                    result = new PlasmaClientConfig();
                    break;
                default:
                    throw new NotSupportedException();
            }

            serializer.Populate(jObject.CreateReader(), result);

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Blockchain client configuration read from json Hoard config file
    /// </summary>
    [JsonConverter(typeof(BCClientConfigConverter))]
    [Serializable]
    public abstract class BCClientConfig
    {
        /// <summary>
        /// Type of blockchain client
        /// </summary>
        internal BCClientType Type { get; set; }
    }

    /// <summary>
    /// Ethereum blockchain client configuration data
    /// </summary>
    [Serializable]
    public class EthereumClientConfig : BCClientConfig
    {
        /// <summary>
        /// Constructor of Ethereum blockchain client configuration data
        /// </summary>
        public EthereumClientConfig()
        {
            Type = BCClientType.Ethereum;
        }

        /// <summary>
        /// URL of ethereum network access client (localhost or testnet main access client URL)
        /// </summary>
        public string ClientUrl;
    }

    /// <summary>
    /// Plasma blockchain client configuration data
    /// </summary>
    [Serializable]
    public class PlasmaClientConfig : BCClientConfig
    {
        /// <summary>
        /// Constructor of Plasma blockchain client configuration data
        /// </summary>
        public PlasmaClientConfig()
        {
            Type = BCClientType.Plasma;
        }

        /// <summary>
        /// URL of ethereum network access client (localhost or testnet main access client URL)
        /// </summary>
        public string ClientUrl;

        /// <summary>
        /// URL of Plasma child chain node
        /// </summary>
        public string ChildChainUrl;

        /// <summary>
        /// URL of Plasma watcher
        /// </summary>
        public string WatcherUrl;
    }

    /// <summary>
    /// Abstract blockchain client options
    /// </summary>
    public abstract class BCClientOptions { }

    /// <summary>
    /// Ethereum blockchain client options
    /// </summary>
    public class EthereumClientOptions : BCClientOptions
    {
        /// <summary>
        /// Rpc client - accessor for the Hoard network
        /// </summary>
        public Nethereum.JsonRpc.Client.IClient RpcClient { get; private set; } = null;

        /// <summary>
        /// Creates Ethereum client options object.
        /// </summary>
        /// <param name="rpcClient">JsonRpc client implementation</param>
        public EthereumClientOptions(Nethereum.JsonRpc.Client.IClient rpcClient)
        {
            RpcClient = rpcClient;
        }
    }

    /// <summary>
    /// Plasma blockchain client options
    /// </summary>
    public class PlasmaClientOptions : BCClientOptions
    {
        /// <summary>
        /// Rpc client - accessor for the Hoard network
        /// </summary>
        public Nethereum.JsonRpc.Client.IClient RpcClient { get; private set; } = null;

        /// <summary>
        /// Plasma child chain client of Hoard network
        /// </summary>
        public PlasmaCore.RPC.IClient ChildChainClient { get; private set; } = null;

        /// <summary>
        /// Plasma watcher client of Hoard network
        /// </summary>
        public PlasmaCore.RPC.IClient WatcherClient { get; private set; } = null;

        /// <summary>
        /// Creates Plasma client options object.
        /// </summary>
        /// <param name="rpcClient">JsonRpc client implementation</param>
        /// <param name="childChainClient">Plasma child chain client</param>
        /// <param name="watcherClient">Plasma watcher client</param>
        public PlasmaClientOptions(Nethereum.JsonRpc.Client.IClient rpcClient, PlasmaCore.RPC.IClient childChainClient, PlasmaCore.RPC.IClient watcherClient)
        {
            RpcClient = rpcClient;
            ChildChainClient = childChainClient;
            WatcherClient = watcherClient;
        }
    }
}
