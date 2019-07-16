using Hoard;
using HoardTest;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace HoardTests.RpcModel
{
    public class LegacyRpcClient
    {
        private readonly Uri _baseUrl;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private object _lockObject = new object();
        private const int NUMBER_OF_SECONDS_TO_RECREATE_HTTP_CLIENT = 60;

        public LegacyRpcClient(Uri baseUrl)
            : this(baseUrl, null)
        {
        }

        public LegacyRpcClient(Uri baseUrl,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _baseUrl = baseUrl;
            if (jsonSerializerSettings == null)
            {
                //jsonSerializerSettings = DefaultJsonSerializerSettingsFactory.BuildDefaultJsonSerializerSettings();
            }
            _jsonSerializerSettings = jsonSerializerSettings;

        }

        /// <summary>
        /// Calls WebRequest using Unity model. Must be called from coroutine on main thread. Returns RpcResult<TResult>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public TResult SendRequest<TResult>(RpcRequest request)
        {
            JsonSerializerSettings settings = null;
            var rpcRequestJson = JsonConvert.SerializeObject(request, settings);
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(_baseUrl);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(rpcRequestJson);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    RpcResponse responseObject = JsonConvert.DeserializeObject<RpcResponse>(result, settings);
                    return responseObject.GetResult<TResult>(true, settings);
                }
            }
        }

        public bool CheckConnection()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_baseUrl);
                //request.Method = "HEAD";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString(), e);
            }
        }
    }
}
