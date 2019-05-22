using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PlasmaCore.RPC
{
    public class RpcResponse
    {
        [JsonProperty(propertyName: "version")]
        public string Version { get; protected set; }

        [JsonProperty(propertyName: "success")]
        public bool Success { get; protected set; }

        [JsonProperty(propertyName: "data")]
        public JToken Data { get; protected set; }

        public RpcResponse(JToken data)
        {
            Success = true;
            Data = data;
        }

        public T GetData<T>(bool returnDefaultIfNull = true, JsonSerializerSettings settings = null)
        {
            if (Data == null)
            {
                if (!returnDefaultIfNull && default(T) != null)
                {
                    throw new Exception("Unable to convert the result (null) to type " + typeof(T));
                }
                return default(T);
            }
            try
            {
                if (settings == null)
                {
                    return Data.ToObject<T>();
                }
                else
                {
                    JsonSerializer jsonSerializer = JsonSerializer.Create(settings);
                    return Data.ToObject<T>(jsonSerializer);
                }
            }
            catch (FormatException ex)
            {
                throw new FormatException("Invalid format when trying to convert the result to type " + typeof(T), ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to convert the result to type " + typeof(T), ex);
            }
        }
    }
}
