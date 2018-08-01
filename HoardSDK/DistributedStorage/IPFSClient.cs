using Hoard.Utils.Base58Check;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Linq;
using System.Text;
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

        private byte fnCode;
        private byte digestSize;

        public IPFSClient(string uploadClientUrl = "http://localhost:5001", string downloadClientUrl = "http://localhost:8080", 
                            byte fnCode = 18, byte digestSize = 32)
        {
            uploadClient = new RestClient(uploadClientUrl);
            downloadClient = new RestClient(downloadClientUrl);

            this.fnCode = fnCode;
            this.digestSize = digestSize;
        }
        
        public async Task<byte[]> DownloadBytesAsync(string address)
        {
            byte[] bytes = new byte[digestSize + 2];
            bytes[0] = fnCode;
            bytes[1] = digestSize;
            Encoding.Unicode.GetBytes(address, 0, address.Length, bytes, 2);

            string hash = Base58CheckEncoding.EncodePlain(bytes);
            RestRequest downloadRequest = new RestRequest("/ipfs/" + hash, Method.GET);
//            return (await downloadClient.ExecuteGetTaskAsync<byte[]>(downloadRequest)).Data;
            return downloadClient.DownloadData(downloadRequest);
        }

        public async Task<string> UploadAsync(byte[] data)
        {
            RestRequest request = new RestRequest("/api/v0/add", Method.POST);
            request.AddFile("file", data, "file", "application/octet-stream");

            //TODO: Make this async
//            IRestResponse response = await uploadClient.ExecutePostTaskAsync(request);
            IRestResponse response = uploadClient.Execute(request);
            if (response.ErrorException != null)
            {
                // TODO: throw some kind of custom exception on unsuccessful upload
                throw new ApplicationException();
            }

            string hash = JsonConvert.DeserializeObject<UploadResponse>(response.Content).Hash;
            byte[] address = Base58CheckEncoding.DecodePlain(hash).Skip(2).ToArray();
            return Encoding.Unicode.GetString(address);
        }
    }
}
