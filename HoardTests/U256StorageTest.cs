using Hoard.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests
{
    public class U256StorageTest
    {
        public U256StorageTest()
        {
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void PackAndUnpackData()
        {
            const bool b1 = true;
            const byte u8_1 = 0x13;
            const byte u8_2 = 0xff;
            const UInt64 u64_1 = 0xff00ff00ff00ff00;
            const UInt16 u16_1 = 0xaabb;
            const UInt32 u32_1 = 0xaaffaaff;
            const bool b2 = true;
            const bool b3 = false;
            const bool b4 = true;
            const UInt32 u32_2 = 0xaaffccff;

            U256StoragePacker u256StoragePacker = new Hoard.Utils.U256StoragePacker();
            u256StoragePacker.PackBool(b1);
            u256StoragePacker.PackUInt8(u8_1);
            u256StoragePacker.PackUInt8(u8_2);
            u256StoragePacker.PackUInt64(u64_1);
            u256StoragePacker.PackUInt16(u16_1);
            u256StoragePacker.PackUInt32(u32_1);
            u256StoragePacker.PackBool(b2);
            u256StoragePacker.PackBool(b3);
            u256StoragePacker.PackBool(b4);
            u256StoragePacker.PackUInt32(u32_2);
            u256StoragePacker.ExportToFile(@"state.json");

            U256StorageUnpacker u256StorageUnpacker = new Hoard.Utils.U256StorageUnpacker(u256StoragePacker.State);
            Assert.Equal(u256StorageUnpacker.UnpackBool(), b1);
            Assert.Equal(u256StorageUnpacker.UnpackUInt8(), u8_1);
            Assert.Equal(u256StorageUnpacker.UnpackUInt8(), u8_2);
            Assert.Equal(u256StorageUnpacker.UnpackUInt64(), u64_1);
            Assert.Equal(u256StorageUnpacker.UnpackUInt16(), u16_1);
            Assert.Equal(u256StorageUnpacker.UnpackUInt32(), u32_1);
            Assert.Equal(u256StorageUnpacker.UnpackBool(), b2);
            Assert.Equal(u256StorageUnpacker.UnpackBool(), b3);
            Assert.Equal(u256StorageUnpacker.UnpackBool(), b4);
            Assert.Equal(u256StorageUnpacker.UnpackUInt32(), u32_2);

            BigInteger state2 = new BigInteger();
            U256StorageUnpacker.ImportFromFile(@"state.json", ref state2);
            U256StorageUnpacker u256StorageUnpacker2 = new Hoard.Utils.U256StorageUnpacker(state2);
            Assert.Equal(u256StorageUnpacker2.UnpackBool(), b1);
            Assert.Equal(u256StorageUnpacker2.UnpackUInt8(), u8_1);
            Assert.Equal(u256StorageUnpacker2.UnpackUInt8(), u8_2);
            Assert.Equal(u256StorageUnpacker2.UnpackUInt64(), u64_1);
            Assert.Equal(u256StorageUnpacker2.UnpackUInt16(), u16_1);
            Assert.Equal(u256StorageUnpacker2.UnpackUInt32(), u32_1);
            Assert.Equal(u256StorageUnpacker2.UnpackBool(), b2);
            Assert.Equal(u256StorageUnpacker2.UnpackBool(), b3);
            Assert.Equal(u256StorageUnpacker2.UnpackBool(), b4);
            Assert.Equal(u256StorageUnpacker2.UnpackUInt32(), u32_2);
        }
    }
}
