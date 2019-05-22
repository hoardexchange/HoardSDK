using Newtonsoft.Json;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for an unsuccessful request
    /// </summary>
    public class ErrorData
    {
        public class MessageData
        {
            [JsonProperty(propertyName: "error_key")]
            public string ErrorKey { get; private set; }
        }

        [JsonProperty(propertyName: "object")]
        public string Object { get; private set; }

        [JsonProperty(propertyName: "code")]
        public string Code { get; private set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; private set; }

        [JsonProperty(propertyName: "messages")]
        public MessageData[] Messages { get; private set; }
    }
}
