using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public struct ItemCRC
    {
        public ulong crc;
    }

    public class ItemData
    {
        public byte[] BinaryData;
        public ItemCRC DataCRC;
    }

    public class GameAsset
    {
        public string Symbol { get; private set; } = null;
        public string Name { get; private set; } = null;
        public string ContractAddress { get; private set; } = null;
        public ulong TotalSuplly { get; private set; }
        public ulong AssetId { get; private set; }
        public string AssetType { get; private set; }

        public GameAsset(string symbol, string name, string contractAddress, ulong totalSuplly, ulong assetId, string assetType)
        {
            Name = name;
            Symbol = symbol;
            ContractAddress = contractAddress;
            TotalSuplly = totalSuplly;
            AssetId = assetId;
            AssetType = assetType;
        }
    }

    public struct GameAssetBalance
    {
        public GameAsset GameAsset;
        public readonly ulong Balance;

        public GameAssetBalance(GameAsset ga, ulong balance)
        {
            GameAsset = ga;
            Balance = balance;
        }
    }   

}
