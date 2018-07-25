using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace Hoard.DistributedStorage
{
    public class IPFSClient : IDistributedStorageClient
    {
        public class UploadResponse
        {
            public string Name { get; set; }
            public string Hash { get; set; }
        }

        private RestClient uploadClient = null;
        private RestClient downloadClient = null;

        IPFSClient(string uploadClientUrl, string downloadClientUrl)
        {
            uploadClient = new RestClient(uploadClientUrl);
            downloadClient = new RestClient(downloadClientUrl);
        }

        public async Task<byte[]> DownloadBytesAsync(ulong address)
        {
            // FIXME: encode address to base58
            string hash = "";
            RestRequest downloadRequest = new RestRequest("/ipfs/" + hash, Method.GET);
            return downloadClient.DownloadData(downloadRequest);
        }

        public async Task<ulong> UploadAsync(byte[] data)
        {
            RestRequest request = new RestRequest("/api/v0/add", Method.POST);
            request.AddFile("file", data, "file", "application/octet-stream");

            IRestResponse response = uploadClient.Execute(request);
            if (response.ErrorException != null)
            {
                // TODO: throw some kind of custom exception on unsuccessful upload
                throw new ApplicationException();
            }

            string hash = JsonConvert.DeserializeObject<UploadResponse>(response.Content).Hash;

            //FIXME: return hash encoded as base58
            ulong address = 0;

            return address;
        }
    }
}
