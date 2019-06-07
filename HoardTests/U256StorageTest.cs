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
            const ulong u64_1 = 0xff00ff00ff00ff00;
            const ushort u16_1 = 0xbaab;
            const uint u32_1 = 0xbaffbaff;
            const bool b2 = true;
            const bool b3 = false;
            const bool b4 = true;
            const uint u32_2 = 0xaaffccff;

            //pack values into state
            U256StoragePacker u256StoragePacker = new U256StoragePacker();
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

            //unpack values and check
            U256StorageUnpacker u256StorageUnpacker = new U256StorageUnpacker(u256StoragePacker.State);
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
        }
    }
}
