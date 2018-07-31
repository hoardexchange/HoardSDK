using RestSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hoard.DistributedStorage
{
    public class SwarmClient : IDistributedStorageClient
    {
        private RestClient client = null;

        SwarmClient(string baseUrl)
        {
            client = new RestClient(baseUrl);
        }

        public async Task<byte[]> DownloadBytesAsync(ulong address)
        {
            // FIXME: encode address to base58
            string hash = "";
            RestRequest downloadRequest = new RestRequest("/bzz:/" + hash + "/file", Method.GET);
            return client.DownloadData(downloadRequest);
        }

        public async Task<ulong> UploadAsync(byte[] data)
        {
            RestRequest request = new RestRequest("/bzz:/", Method.POST);
            request.AddFile("file", data, "file", "application/octet-stream");
            IRestResponse response = client.Execute(request);

            if (response.ErrorException != null)
            {
                // TODO: throw some kind of custom exception on unsuccessful upload
                throw new ApplicationException();
            }

            string hash = response.Content;
            //FIXME: return hash encoded as base58
            ulong address = 0;
            return address;
        }
    }
}
