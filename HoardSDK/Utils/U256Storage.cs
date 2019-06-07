using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Hoard.Utils
{
    /// <summary>
    /// Available storage types
    /// </summary>
    public enum U256StorageDataType
    {
        /// <summary>
        /// unsigned short integer
        /// </summary>
        UInt16 = 0,
        /// <summary>
        /// unsigned 32bit integer
        /// </summary>
        UInt32,
        /// <summary>
        /// unsigned 64bit integer
        /// </summary>
        UInt64,
        /// <summary>
        /// Bool
        /// </summary>
        Bool,
        /// <summary>
        /// Single byte
        /// </summary>
        Byte,

        /// <summary>
        /// Maximum count of supported types
        /// </summary>
        MaxStorageSize = 256,
    }

    /// <summary>
    /// Item state storage description
    /// </summary>
    public struct U256StorageDescription
    {
        /// <summary>
        /// Type of the value
        /// </summary>
        public U256StorageDataType Type;
        /// <summary>
        /// Size in bytes of this value
        /// </summary>
        public int Size;
    }

    /// <summary>
    /// Item state storage packer
    /// </summary>
    public class U256StoragePacker
    {
        private int ActualShift;

        /// <summary>
        /// Item state
        /// </summary>
        public BigInteger State { get; private set; }

        /// <summary>
        /// Item state storage constructor
        /// </summary>
        public U256StoragePacker()
        {
            State = BigInteger.Zero;
            ActualShift = 0;
        }

        /// <summary>
        /// Return available bits in storage
        /// </summary>
        /// <returns></returns>
        public int GetBitsToPack()
        {
            return (int)U256StorageDataType.MaxStorageSize - ActualShift;
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param name="value"> value </param>
        public void PackUInt8(byte value)
        {
            State |= new BigInteger(value) << ActualShift;
            ActualShift += 8;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param name="value"> value </param>
        public void PackUInt16(UInt16 value)
        {
            State |= new BigInteger(value) << ActualShift;
            ActualShift += 16;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param name="value"> value </param>
        public void PackUInt32(UInt32 value)
        {
            State |= new BigInteger(value) << ActualShift;
            ActualShift += 32;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param name="value"> value </param>
        public void PackUInt64(ulong value)
        {
            State |= new BigInteger(value) << ActualShift;
            ActualShift += 64;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param name="value"> value </param>
        public void PackBool(bool value)
        {
            State |= new BigInteger((byte)(value == true ? 0xff : 0x00)) << ActualShift;
            ActualShift += 8;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
        }
    }

    /// <summary>
    /// Item state storage unpacker
    /// </summary>
    public class U256StorageUnpacker
    {
        private int ActualShift;

        /// <summary>
        /// Item state
        /// </summary>
        public BigInteger State { get; private set; }

        /// <summary>
        /// Item state storage constructor
        /// </summary>
        /// <param state="state"> the state we want to modify </param>
        public U256StorageUnpacker(BigInteger state)
        {
            State = state;
            ActualShift = 0;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public byte UnpackUInt8()
        {
            byte value = (byte)((State >> ActualShift) & 0xff);
            ActualShift += 8;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
            return value;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public ushort UnpackUInt16()
        {
            ushort value = (ushort)((State >> ActualShift) & 0xffff);
            ActualShift += 16;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
            return value;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public uint UnpackUInt32()
        {
            uint value = (uint)((State >> ActualShift) & 0xffffffff);
            ActualShift += 32;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
            return value;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public ulong UnpackUInt64()
        {
            ulong value = (ulong)((State >> ActualShift) & 0xffffffffffffffff);
            ActualShift += 64;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
            return value;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public bool UnpackBool()
        {
            return UnpackUInt8() == 0xff ? true : false;
        }
    }
}
