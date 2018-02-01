using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public struct Item
    {
        public ulong ID;
        public ulong Count;
    }

    public struct ItemCRC
    {
        public ulong crc;
    }

    public class ItemData
    {
        public byte[] BinaryData;
        public ItemCRC DataCRC;
    }

    public class GameItem
    {
    }

    public class Coin
    {
        public ulong Count { get; private set; } = 0;
        public string Symbol { get; private set; } = null;
        public string Name { get; private set; } = null;

        public Coin(string symbol, string name, ulong count)
        {
            Symbol = symbol;
            Name = name;
            Count = count;
        }
    }
}
