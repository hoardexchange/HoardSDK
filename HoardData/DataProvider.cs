﻿using System;
using System.Collections.Generic;

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
        public int ver;
        public Dictionary<ulong, AssetFile> assets = new Dictionary<ulong, AssetFile>(); // assetId to cached AssetFile
    }

    public class DataProvider : Provider
    {
        private string propertyName = "file";

        private const int DataStorageService_Ver = 0; // cache version

        private GBDesc GBDesc = null;
        private string CacheBasePath = null;
        private GameDataInfo GameDataInfo = null;
        private DataStorageBackend DataStorageBackend = null;

        public DataProvider(HoardService hoard)
        {
            this.GBDesc = hoard.GameBackendDesc;

            CacheBasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hoard", "files_cache");

            byte[] gameDataInfoSerialized = DataStorageUtils.LoadFromDisk(GetGameDataPath());
            if (gameDataInfoSerialized != null)
            {
                GameDataInfo = DataStorageUtils.Deserialize(gameDataInfoSerialized);
                if (GameDataInfo.ver != DataStorageService_Ver)
                {
                    GameDataInfo = new GameDataInfo();
                }
            }
            else
            {
                GameDataInfo = new GameDataInfo();
            }

            //DataStorageBackend = new DataStorageBackendLocal("e:/hoard/DSS/server/"); // local test (storege on local disk)
            DataStorageBackend = new DataStorageBackendHoard(hoard.GameBackendClient);
            DataStorageBackend.Init(hoard.GameBackendDesc);
        }

        private string GetGameCachePath()
        {
            return CacheBasePath + "/" + GBDesc.GameID;
        }

        private string GetGameDataPath()
        {
            return GetGameCachePath() + "/gameDataInfo.dat";
        }

        public Result Store(ulong assetId, string diskFilePath)
        {
            byte[] data = DataStorageUtils.LoadFromDisk(diskFilePath);
            if (data == null)
            {
                return new Result("Unable to get data from file");
            }
            return DataStorageBackend.UploadDataToServer(assetId, data);
        }

        public Result Store(ulong assetId, byte[] data)
        {
            return DataStorageBackend.UploadDataToServer(assetId, data);
        }

        public Result Load(ulong assetId, out byte[] data)
        {
            AssetFile assetFile = null;
            bool existsInCache = GameDataInfo.assets.TryGetValue(assetId, out assetFile);

            // get hash from the server
            string hash = null;
            Result hashResult = DataStorageBackend.LoadHashFromServer(assetId, out hash);
            bool existsOnServer = (hashResult.Success);

            if (!existsOnServer)
            {
                data = null;
                return hashResult;
            }

            // check if we have this asset in the local cache
            if (existsInCache && assetFile.hash == hash)
            {
                data = DataStorageUtils.LoadFromDisk(GetGameCachePath() + "/" + assetId);
                if (data != null)
                {
                    return new Result();
                }
            }

            // get data from the server
            Result dataResult = DataStorageBackend.LoadDataFromServer(assetId, out data);
            if (!dataResult.Success)
                return dataResult;

            // store it in the local cache
            DataStorageUtils.SaveToDisk(GetGameCachePath() + "/" + assetId, data);

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
            DataStorageUtils.SaveToDisk(GetGameDataPath(), gameDataInfoSerialized);

            return new Result();
        }

        byte[] Decrypt(byte[] data)
        {
            //TODO
            return data;
        }

        /* Provider interface implementation */

        override public string[] getPropertyNames()
        {
            return new string[1] { propertyName };
        }

        override public Result getItems(out List<GameAsset> items)
        {
            items = null;
            return new Result("Not supported");
        }

        override public Result getProperties(GameAsset item)
        {
            byte[] data = null;

            Result result = Load(item.AssetId, out data);
            if (!result.Success)
                return result;

            item.Properties.Set(propertyName, data);

            return result;
        }
    }
}
