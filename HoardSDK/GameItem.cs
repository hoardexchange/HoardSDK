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

    public class Coin
    {
        public string Symbol { get; private set; } = null;
        public string Name { get; private set; } = null;
        public string ContractAddress { get; private set; } = null;
        public ulong TotalSuplly { get; private set; }

        public Coin(string symbol, string name, string contractAddress, ulong totalSuplly)
        {
            Name = name;
            Symbol = symbol;
            ContractAddress = contractAddress;
            TotalSuplly = totalSuplly;
        }
    }

    public class CoinBalance
    {
        public Coin Coin { get; private set; } = null;
        public ulong Balance { get; private set; } = 0;

        public CoinBalance(Coin coin, ulong balance)
        {
            Coin = coin;
            Balance = balance;
        }
    }

    public class GameAsset
    {
        public string Name { get; private set; } = null;
        public string Symbol { get; private set; } = null;
        public string ContractAddress { get; private set; } = null;
        public ulong TotalSuplly { get; private set; }

        public ulong AssetId { get; private set; }

        public GameAsset(string name, string symbol, string contractAddress, ulong totalSuplly, ulong assetId)
        {
            Name = name;
            Symbol = symbol;
            ContractAddress = contractAddress;
            TotalSuplly = totalSuplly;
            AssetId = assetId;
        }
    }

    public class GameAssetBalance
    {
        public GameAsset GameAsset { get; private set; } = null;
        public ulong Balance { get; private set; } = 0;

        public GameAssetBalance(GameAsset gameAsset, ulong balance)
        {
            GameAsset = gameAsset;
            Balance = balance;
        }
    }   

}
