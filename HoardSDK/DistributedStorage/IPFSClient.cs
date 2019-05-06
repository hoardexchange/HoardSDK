using Hoard.Utils.Base58Check;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.DistributedStorage
{
    /// <summary>
    /// Access to IPFS data base
    /// </summary>
    public class IPFSClient : IDistributedStorageClient
    {
        private class UploadResponse
        {
            public string Name { get; set; }
            public string Hash { get; set; }
        }

        private RestClient uploadClient = null;
        private RestClient downloadClient = null;

        private byte fnCode;
        private byte digestSize;

        /// <summary>
        /// Creates new instance of IPFS data base client
        /// </summary>
        /// <param name="uploadClientUrl">upload access point</param>
        /// <param name="downloadClientUrl">dwonload access point</param>
        /// <param name="fnCode">IPFS client configuration param</param>
        /// <param name="digestSize">IPFS client configuration param</param>
        public IPFSClient(string uploadClientUrl = "http://localhost:5001", string downloadClientUrl = "http://localhost:8080", 
                            byte fnCode = 18, byte digestSize = 32)
        {
            if (uploadClientUrl != null && uploadClientUrl != "")
            {
                uploadClient = new RestClient(uploadClientUrl);
                uploadClient.AutomaticDecompression = false;
                uploadClient.Timeout = 3000;
            }

            if (downloadClientUrl != null && downloadClientUrl != "")
            {
                downloadClient = new RestClient(downloadClientUrl);
                downloadClient.AutomaticDecompression = false;
                downloadClient.Timeout = 3000;
            }

            this.fnCode = fnCode;
            this.digestSize = digestSize;
        }
        
        /// <inheritdoc/>
        public async Task<byte[]> DownloadBytesAsync(byte[] address)
        {
            if (downloadClient != null)
            {
                byte[] bytes = new byte[digestSize + 2];
                bytes[0] = fnCode;
                bytes[1] = digestSize;
                address.CopyTo(bytes, 2);
                //Encoding.Unicode.GetBytes(address, 0, address.Length, bytes, 2);

                string hash = Base58CheckEncoding.EncodePlain(bytes);
                RestRequest downloadRequest = new RestRequest("/ipfs/" + hash, Method.GET);
                downloadRequest.AddDecompressionMethod(System.Net.DecompressionMethods.None);

                IRestResponse response = await downloadClient.ExecuteGetTaskAsync(downloadRequest);
                if (response.ErrorException != null)
                {
                    // TODO: throw some kind of custom exception on unsuccessful upload
                    throw new Exception();
                }

                return response.RawBytes;
            }
            return null;
        }

        /// <inheritdoc/>
        public async Task<byte[]> UploadAsync(byte[] data)
        {
            if (uploadClient != null)
            {
                RestRequest request = new RestRequest("/api/v0/add", Method.POST);
                request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
                request.AddFile("file", data, "file", "application/octet-stream");

                IRestResponse response = await uploadClient.ExecutePostTaskAsync(request);
                if (response.ErrorException != null)
                {
                    // TODO: throw some kind of custom exception on unsuccessful upload
                    throw new Exception();
                }

                string hash = JsonConvert.DeserializeObject<UploadResponse>(response.Content).Hash;
                return Base58CheckEncoding.DecodePlain(hash).Skip(2).ToArray();
            }
            return null;
        }
    }
}
