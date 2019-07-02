using Newtonsoft.Json;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// RPC token type data 
    /// </summary>
    public class TokenTypeData
    {
        [JsonProperty(propertyName: "currency")]
        public string Currency { get; private set; }

        [JsonProperty(propertyName: "type")]
        public string Type { get; private set; }
    }
}
