using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task PackAndUnpackData()
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

            Hoard.Utils.U256Storage u256Storage = new Hoard.Utils.U256Storage();
            u256Storage.PackBool(b1);
            u256Storage.PackUInt8(u8_1);
            u256Storage.PackUInt8(u8_2);
            u256Storage.PackUInt64(u64_1);
            u256Storage.PackUInt16(u16_1);
            u256Storage.PackUInt32(u32_1);
            u256Storage.PackBool(b2);
            u256Storage.PackBool(b3);
            u256Storage.PackBool(b4);
            u256Storage.PackUInt32(u32_2);

            Assert.Equal(u256Storage.UnpackBool(), b1);
            Assert.Equal(u256Storage.UnpackUInt8(), u8_1);
            Assert.Equal(u256Storage.UnpackUInt8(), u8_2);

            Assert.Equal(u256Storage.UnpackUInt64(), u64_1);
            Assert.Equal(u256Storage.UnpackUInt16(), u16_1);
            Assert.Equal(u256Storage.UnpackUInt32(), u32_1);
            Assert.Equal(u256Storage.UnpackBool(), b2);
            Assert.Equal(u256Storage.UnpackBool(), b3);
            Assert.Equal(u256Storage.UnpackBool(), b4);
            Assert.Equal(u256Storage.UnpackUInt32(), u32_2);
        }
    }
}
