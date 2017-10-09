using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public struct ItemID
    {
        public ulong ID;
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
}
