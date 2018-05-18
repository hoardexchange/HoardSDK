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

        abstract public string LoadHashFromServer(ulong assetId);

        abstract public byte[] LoadDataFromServer(ulong assetId);

        abstract public void UploadDataToServer(ulong assetId, byte[] data);
    }

    //DataStorageBackendLocal is for test purpose only!
    public class DataStorageBackendLocal : DataStorageBackend
    {
        private string StorageBasePath;

        public DataStorageBackendLocal(string StorageBasePath)
        {
            this.StorageBasePath = StorageBasePath;
        }

        public override string LoadHashFromServer(ulong assetId)
        {
            byte[] data = LoadDataFromServer(assetId);
            if (data == null)
                return null;

            return ComputeHash(data);
        }

        public override byte[] LoadDataFromServer(ulong assetId)
        {
            byte[] data = DataStorageUtils.LoadFromDisk(StorageBasePath + "/" + GBDesc.GameID + "/" + assetId);
            return data;
        }

        public override void UploadDataToServer(ulong assetId, byte[] data)
        {
            DataStorageUtils.SaveToDisk(StorageBasePath + "/" + GBDesc.GameID + "/" + assetId, data);
            return;
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

        public override string LoadHashFromServer(ulong assetId)
        {
            var task = Client.GetJson("/res/?game_id=" + GBDesc.GameID + "&asset_id=" + assetId, null);
            task.Wait();
            var jsonStr = task.Result;

            AssetHash[] gameDataInfo = JsonConvert.DeserializeObject<AssetHash[]>(jsonStr);
            if (gameDataInfo.Length>0)
            {
                return gameDataInfo[0].hash;
            }

            return null;
        }

        public override byte[] LoadDataFromServer(ulong assetId)
        {
            byte[] data = null;
            var task = Client.GetRawData("/res_download/" + GBDesc.GameID + "/" + assetId + "/");
            task.Wait();
            data = task.Result;
            return data;
        }

        public override void UploadDataToServer(ulong assetId, byte[] data)
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
        }
    }
}
