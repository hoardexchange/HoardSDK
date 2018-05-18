using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using RestSharp;

namespace Hoard
{
    public class AssetFile
    {
        public ulong assetId;
        public string hash;
        public ulong size;
    }

    public class GameDataInfo
    {
        public Dictionary<ulong, AssetFile> assets = new Dictionary<ulong, AssetFile>(); // assetId to cached AssetFile
    }

    public class DataStorageService
    {
        private GBDesc GBDesc = null;
        private string CacheBasePath = null;
        private GameDataInfo GameDataInfo = null;
        private DataStorageBackend DataStorageBackend = null;

        public DataStorageService(GBClient client, HoardService hoard)
        {
            this.GBDesc = hoard.GameBackendDesc;

            CacheBasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hoard", "files_cache");

            byte[] gameDataInfoSerialized = DataStorageUtils.LoadFromDisk(CacheBasePath + "/" + GBDesc.GameID + "/gameDataInfo.dat");
            if (gameDataInfoSerialized != null)
            {
                GameDataInfo = DataStorageUtils.Deserialize(gameDataInfoSerialized);
            }
            else
            {
                GameDataInfo = new GameDataInfo();
            }

            //DataStorageBackend = new DataStorageBackendLocal("e:/hoard/DSS/server/"); // local test (storege on local disk)
            DataStorageBackend = new DataStorageBackendHoard(client);
            DataStorageBackend.Init(hoard.GameBackendDesc);
        }

        public void Store(ulong assetId, string diskFilePath)
        {
            byte[] data = DataStorageUtils.LoadFromDisk(diskFilePath);

            DataStorageBackend.UploadDataToServer(assetId, data);
        }

        public void Store(ulong assetId, byte[] data)
        {
            DataStorageBackend.UploadDataToServer(assetId, data);
        }

        public byte[] Load(ulong assetId)
        {
            AssetFile assetFile = null;
            bool existsInCache = GameDataInfo.assets.TryGetValue(assetId, out assetFile);

            // get hash from the server
            string hash = DataStorageBackend.LoadHashFromServer(assetId);
            bool existsOnServer = (hash!=null);

            if (!existsOnServer)
                return null;

            // check if we have this asset in the local cache
            if (existsInCache && assetFile.hash == hash)
            {
                byte[] cachedData = DataStorageUtils.LoadFromDisk(CacheBasePath + "/" + GBDesc.GameID + "/" + assetId);
                if (cachedData != null)
                    return cachedData;
            }

            // get data from the server
            byte[] data = DataStorageBackend.LoadDataFromServer(assetId);
            if (data == null)
                return null;

            // store it in the local cache
            DataStorageUtils.SaveToDisk(CacheBasePath + "/" + GBDesc.GameID + "/" + assetId, data);

            // write meta info about this asset
            if (existsInCache)
            {
                GameDataInfo.assets.Remove(assetId);
            }
            assetFile = new AssetFile();
            assetFile.assetId = assetId;
            assetFile.hash = hash;
            assetFile.size = (ulong)data.Length;

            GameDataInfo.assets[assetId] = assetFile;

            byte[] gameDataInfoSerialized = DataStorageUtils.Serialize(GameDataInfo);
            DataStorageUtils.SaveToDisk(CacheBasePath + "/" + GBDesc.GameID + "/gameDataInfo.dat", gameDataInfoSerialized);

            return data;
        }
    } 
}
