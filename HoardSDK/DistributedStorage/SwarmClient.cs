﻿using RestSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hoard.DistributedStorage
{
    public class SwarmClient : IDistributedStorageClient
    {
        private RestClient client = null;

        public SwarmClient(string baseUrl)
        {
            client = new RestClient(baseUrl);
            client.AutomaticDecompression = false;
        }

        public async Task<byte[]> DownloadBytesAsync(byte[] address)
        {
            RestRequest downloadRequest = new RestRequest("/bzz:/" + address + "/file", Method.GET);
            downloadRequest.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            return (await client.ExecuteGetTaskAsync<byte[]>(downloadRequest)).Data;
        }

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