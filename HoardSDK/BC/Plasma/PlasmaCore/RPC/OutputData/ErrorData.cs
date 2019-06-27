using Newtonsoft.Json;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// Response class for an unsuccessful request
    /// </summary>
    [JsonObject]
    public class ErrorData
    {
        /// <summary>
        /// Message data with error key
        /// </summary>
        public class MessageData
        {
            /// <summary>
            /// The key
            /// </summary>
            [JsonProperty(propertyName: "error_key")]
            public string ErrorKey { get; private set; }
        }

        /// <summary>
        /// Arbitrary error object
        /// </summary>
        [JsonProperty(propertyName: "object")]
        public string Object { get; private set; }

        /// <summary>
        /// Error code
        /// </summary>
        [JsonProperty(propertyName: "code")]
        public string Code { get; private set; }

        /// <summary>
        /// Description of error
        /// </summary>
        [JsonProperty(propertyName: "description")]
        public string Description { get; private set; }

        /// <summary>
        /// List of messages output when error happened
        /// </summary>
        [JsonProperty(propertyName: "messages")]
        public MessageData[] Messages { get; private set; }

        [JsonConstructor]
        private ErrorData() { }
    }
}
