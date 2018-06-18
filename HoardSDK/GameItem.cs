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

    // Props holds set of values identified individually by string, it can by anything like single values, custom objects or binary data
    public class Props
    {
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public object Get(string propertyName)
        {
            object prop;
            Properties.TryGetValue(propertyName, out prop);
            return prop;
        }

        public void Set(string propertyName, object propertyValue)
        {
            Properties[propertyName] = propertyValue;
        }
    }

    public class Instance
    {
        public Props Properties { get; set; } = new Props();
    }

    public class GameAsset
    {
        public string Symbol { get; private set; } = null;
        public string Name { get; private set; } = null;
        public string ContractAddress { get; private set; } = null;
        public ulong TotalSuplly { get; private set; }
        public ulong AssetId { get; private set; }
        public string AssetType { get; private set; }

        public BC.Contracts.GameAssetContract Contract { get; private set; } = null;

        // GameAsset's properties
        public Props Properties { get; set; } = new Props();

        // GameAsset's Instances contains properties of particular item identified by tokenId (see ERC721), used for NFT tokens
        public Dictionary<string, Instance> Instances { get; set; } = null;

        public GameAsset(string symbol, string name, BC.Contracts.GameAssetContract contract, ulong totalSuplly, ulong assetId, string assetType)
        {
            Name = name;
            Symbol = symbol;
            Contract = contract;
            ContractAddress = (contract != null) ? contract.Address : null;
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
