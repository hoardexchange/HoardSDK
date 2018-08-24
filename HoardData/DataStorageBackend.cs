﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Hoard
{
    abstract public class DataStorageBackend
    {
        protected GameID GameID = null;

        public void Init(GameID gameID)
        {
            this.GameID = gameID;
        }

        protected string ComputeHash(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        abstract public Result LoadHashFromServer(ulong assetId, out string hash);

        abstract public Result LoadDataFromServer(ulong assetId, out byte[] data);

        abstract public Result UploadDataToServer(ulong assetId, byte[] data);
    }

    //DataStorageBackendLocal is for test purpose only!
    public class DataStorageBackendLocal : DataStorageBackend
    {
        private string StorageBasePath;

        public DataStorageBackendLocal(string StorageBasePath)
        {
            this.StorageBasePath = StorageBasePath;
        }

        public override Result LoadHashFromServer(ulong assetId, out string hash)
        {
            byte[] data = null;
            Result result = LoadDataFromServer(assetId, out data);
            if (!result.Success)
            {
                hash = null;
                return new Result("No data for given assetId");
            }

            hash = ComputeHash(data);
            return new Result();
        }

        public override Result LoadDataFromServer(ulong assetId, out byte[] data)
        {
            data = DataStorageUtils.LoadFromDisk(StorageBasePath + "/" + GameID.ID + "/" + assetId);
            return new Result();
        }

        public override Result UploadDataToServer(ulong assetId, byte[] data)
        {
            DataStorageUtils.SaveToDisk(StorageBasePath + "/" + GameID.ID + "/" + assetId, data);
            return new Result();
        }
    }

    public class DataStorageBackendHoard : DataStorageBackend
    {
        private RestClient Client = null;

        public DataStorageBackendHoard(RestClient Client)
        {
            this.Client = Client;
            System.Diagnostics.Debug.Assert(Client.CookieContainer != null,"[ERROR] RestClient does not have any cookies. All requests won't be authenticated!");
        }

        public class AssetHash
        {
            public string hash;
        }

        public override Result LoadHashFromServer(ulong assetId, out string hash)
        {
            var task = Get("/res/?game_id=" + GameID.ID + "&asset_id=" + assetId);
            task.Wait();
            var task_result = task.Result;

            hash = null;
            if (task_result.StatusCode != HttpStatusCode.OK)
                return new Result(task_result.StatusDescription);

            var jsonStr = task_result.Content;

            AssetHash[] gameDataInfo = JsonConvert.DeserializeObject<AssetHash[]>(jsonStr);
            if (gameDataInfo.Length>0)
            {
                hash = gameDataInfo[0].hash;
                if (hash!=null)
                    return new Result();
            }

            hash = null;
            return new Result("Error retrieving asset hash");
        }

        public override Result LoadDataFromServer(ulong assetId, out byte[] data)
        {
            var task = Get("/res_download/" + GameID.ID + "/" + assetId + "/");
            task.Wait();
            var task_result = task.Result;

            data = task_result.RawBytes;

            if (task_result.StatusCode != HttpStatusCode.OK)
                return new Result(task_result.StatusDescription);

            if (data==null)
                return new Result("No data");

            return new Result();
        }

        public override Result UploadDataToServer(ulong assetId, byte[] data)
        {
            // delete old resource
            var delete_task = Delete("/res_delete/" + GameID.ID + "/" + assetId + "/");
            delete_task.Wait();
            var delete_task_result = delete_task.Result;

            // create new one
            Parameter gameId_Param = new Parameter();
            gameId_Param.Name = "game_id";
            gameId_Param.Value = GameID.ID;
            gameId_Param.Type = ParameterType.GetOrPost;

            Parameter assetId_Param = new Parameter();
            assetId_Param.Name = "asset_id";
            assetId_Param.Value = assetId;
            assetId_Param.Type = ParameterType.GetOrPost;

            Parameter hash_Param = new Parameter();
            hash_Param.Name = "hash";
            hash_Param.Value = ComputeHash(data);
            hash_Param.Type = ParameterType.GetOrPost;

            Parameter[] parameters = { gameId_Param, assetId_Param, hash_Param };
            var create_task = PostWithFile("/res/", parameters, data);
            create_task.Wait();
            var create_task_response = create_task.Result;

            if (create_task_response.StatusCode != HttpStatusCode.Created)
                return new Result(create_task_response.StatusDescription);
            else
                return new Result();
        }

#region private methods

        private void PrepareRequest(RestRequest req)
        {
            //TODO: check this:
            //req.AddHeader("X-CSRFToken", csrfToken);
        }

        private async Task<IRestResponse> Get(string url)
        {
            var request = new RestRequest(url, Method.GET);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);

            PrepareRequest(request);

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false); ;
            return response;
        }

        private async Task<string> Delete(string url)
        {
            var request = new RestRequest(url, Method.DELETE);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);

            PrepareRequest(request);

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response.Content;
        }


        private async Task<IRestResponse> PostWithFile(string url, Parameter[] parameters, byte[] file)
        {
            var request = new RestRequest(url, Method.POST);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);

            foreach (Parameter p in parameters)
                request.AddParameter(p);
            request.AddFile("file", file, "file");

            PrepareRequest(request);

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response;
        }
        #endregion
    }
}