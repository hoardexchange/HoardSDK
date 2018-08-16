﻿using Hoard.Utils.Base58Check;
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

            uploadClient.AutomaticDecompression = false;
            downloadClient.AutomaticDecompression = false;

            this.fnCode = fnCode;
            this.digestSize = digestSize;
        }
        
        public async Task<byte[]> DownloadBytesAsync(byte[] address)
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
                throw new ApplicationException();
            }

            return response.RawBytes;
        }

        public async Task<byte[]> UploadAsync(byte[] data)
        {
            RestRequest request = new RestRequest("/api/v0/add", Method.POST);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            request.AddFile("file", data, "file", "application/octet-stream");

            IRestResponse response = await uploadClient.ExecutePostTaskAsync(request);
            if (response.ErrorException != null)
            {
                // TODO: throw some kind of custom exception on unsuccessful upload
                throw new ApplicationException();
            }

            string hash = JsonConvert.DeserializeObject<UploadResponse>(response.Content).Hash;
            return Base58CheckEncoding.DecodePlain(hash).Skip(2).ToArray();
           
        }
    }
}
