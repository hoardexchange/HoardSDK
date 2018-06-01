using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace Hoard
{
    abstract public class DataStorageBackend
    {
        protected GBDesc GBDesc = null;

        public void Init(GBDesc GBDesc)
        {
            this.GBDesc = GBDesc;
        }

        protected string ComputeHash(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        abstract public DataStorageService.Result LoadHashFromServer(ulong assetId, out string hash);

        abstract public DataStorageService.Result LoadDataFromServer(ulong assetId, out byte[] data);

        abstract public DataStorageService.Result UploadDataToServer(ulong assetId, byte[] data);
    }

    //DataStorageBackendLocal is for test purpose only!
    public class DataStorageBackendLocal : DataStorageBackend
    {
        private string StorageBasePath;

        public DataStorageBackendLocal(string StorageBasePath)
        {
            this.StorageBasePath = StorageBasePath;
        }

        public override DataStorageService.Result LoadHashFromServer(ulong assetId, out string hash)
        {
            byte[] data = null;
            DataStorageService.Result result = LoadDataFromServer(assetId, out data);
            if (!result.Success)
            {
                hash = null;
                return new DataStorageService.Result("No data for given assetId");
            }

            hash = ComputeHash(data);
            return new DataStorageService.Result();
        }

        public override DataStorageService.Result LoadDataFromServer(ulong assetId, out byte[] data)
        {
            data = DataStorageUtils.LoadFromDisk(StorageBasePath + "/" + GBDesc.GameID + "/" + assetId);
            return new DataStorageService.Result();
        }

        public override DataStorageService.Result UploadDataToServer(ulong assetId, byte[] data)
        {
            DataStorageUtils.SaveToDisk(StorageBasePath + "/" + GBDesc.GameID + "/" + assetId, data);
            return new DataStorageService.Result();
        }
    }

    public class DataStorageBackendHoard : DataStorageBackend
    {
        private GBClient Client = null;

        public DataStorageBackendHoard(GBClient Client)
        {
            this.Client = Client;
        }

        public class AssetHash
        {
            public string hash;
        }

        public override DataStorageService.Result LoadHashFromServer(ulong assetId, out string hash)
        {
            var task = Client.Get("/res/?game_id=" + GBDesc.GameID + "&asset_id=" + assetId);
            task.Wait();
            var task_result = task.Result;

            hash = null;
            if (task_result.StatusCode != HttpStatusCode.OK)
                return new DataStorageService.Result(task_result.StatusDescription);

            var jsonStr = task_result.Content;

            AssetHash[] gameDataInfo = JsonConvert.DeserializeObject<AssetHash[]>(jsonStr);
            if (gameDataInfo.Length>0)
            {
                hash = gameDataInfo[0].hash;
                if (hash!=null)
                    return new DataStorageService.Result();
            }

            hash = null;
            return new DataStorageService.Result("Error retrieving asset hash");
        }

        public override DataStorageService.Result LoadDataFromServer(ulong assetId, out byte[] data)
        {
            var task = Client.Get("/res_download/" + GBDesc.GameID + "/" + assetId + "/");
            task.Wait();
            var task_result = task.Result;

            data = task_result.RawBytes;

            if (task_result.StatusCode != HttpStatusCode.OK)
                return new DataStorageService.Result(task_result.StatusDescription);

            if (data==null)
                return new DataStorageService.Result("No data");

            return new DataStorageService.Result();
        }

        public override DataStorageService.Result UploadDataToServer(ulong assetId, byte[] data)
        {
            // delete old resource
            var delete_task = Client.Delete("/res_delete/" + GBDesc.GameID + "/" + assetId + "/");
            delete_task.Wait();
            var delete_task_result = delete_task.Result;

            // create new one
            Parameter gameId_Param = new Parameter();
            gameId_Param.Name = "game_id";
            gameId_Param.Value = GBDesc.GameID;
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
            var create_task = Client.PostWithFile("/res/", parameters, data);
            create_task.Wait();
            var create_task_response = create_task.Result;

            if (create_task_response.StatusCode != HttpStatusCode.Created)
                return new DataStorageService.Result(create_task_response.StatusDescription);
            else
                return new DataStorageService.Result();
        }
    }
}
