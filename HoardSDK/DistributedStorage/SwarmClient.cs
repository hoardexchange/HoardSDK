using RestSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hoard.DistributedStorage
{
    /// <summary>
    /// Access to Swarm data base
    /// </summary>
    public class SwarmClient : IDistributedStorageClient
    {
        private RestClient client = null;

        /// <summary>
        /// Creates new instance of Swarm data base client
        /// </summary>
        /// <param name="baseUrl">client access point</param>
        public SwarmClient(string baseUrl)
        {
            client = new RestClient(baseUrl);
            client.AutomaticDecompression = false;
        }

        /// <inheritdoc/>
        public async Task<byte[]> DownloadBytesAsync(byte[] address)
        {
            RestRequest downloadRequest = new RestRequest("/bzz:/" + address + "/file", Method.GET);
            downloadRequest.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            return (await client.ExecuteGetTaskAsync<byte[]>(downloadRequest)).Data;
        }

        /// <inheritdoc/>
        public async Task<byte[]> UploadAsync(byte[] data)
        {
            RestRequest request = new RestRequest("/bzz:/", Method.POST);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            request.AddFile("file", data, "file", "application/octet-stream");
            IRestResponse response = await client.ExecutePostTaskAsync(request);

            if (response.ErrorException != null)
            {
                // TODO: throw some kind of custom exception on unsuccessful upload
                throw new ApplicationException();
            }

            return response.RawBytes;
        }
    }
}
