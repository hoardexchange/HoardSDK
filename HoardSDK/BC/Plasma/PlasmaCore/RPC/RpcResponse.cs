using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PlasmaCore.RPC
{
    /// <summary>
    /// RPC response data
    /// </summary>
    public class RPCResponse
    {
        /// <summary>
        /// Response version
        /// </summary>
        [JsonProperty(propertyName: "version")]
        public string Version { get; protected set; }

        /// <summary>
        /// Response success status
        /// </summary>
        [JsonProperty(propertyName: "success")]
        public bool Success { get; protected set; }

        /// <summary>
        /// Response data
        /// </summary>
        [JsonProperty(propertyName: "data")]
        public JToken Data { get; protected set; }

        /// <summary>
        /// Deserializes response data into given type
        /// </summary>
        /// <typeparam name="T">response data type</typeparam>
        /// <param name="settings">json serializer settings</param>
        /// <returns></returns>
        public T GetData<T>(JsonSerializerSettings settings = null)
        {
            if (Data == null || !Success)
                return default(T);

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
                throw new FormatException("Invalid format", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to deserialize object", ex);
            }
        }
    }
}
