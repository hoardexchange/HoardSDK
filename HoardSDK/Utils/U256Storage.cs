#if DEBUG
#define ENABLE_U256STORAGE_VALIDATION
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Hoard.Utils
{
    /// <summary>
    /// Item state storage
    /// </summary>
    public class U256Storage
    {
        private enum DataSize
        {
            UInt8           = 8,
            UInt16          = 16,
            UInt32          = 32,
            UInt64          = 64,
            Bool            = 8,

            MaxStorageSize  = 256,
        }

        private BigInteger Bunch;
        private int ActualPackingShift;
        private int ActualUnpackingShift;

#if ENABLE_U256STORAGE_VALIDATION
        private List<DataSize> DataList = new List<DataSize>();
#endif

        /// <summary>
        /// Item state storage constructor
        /// </summary>
        /// <param state="state"> the state we want to modify </param>
        public U256Storage(ref BigInteger state)
        {
            Bunch = state;
            ActualPackingShift = 0;
            ActualUnpackingShift = 0;
        }

        /// <summary>
        /// Return available bits in storage
        /// </summary>
        /// <returns></returns>
        public int GetBitsToPack()
        {
            return (int)DataSize.MaxStorageSize - ActualPackingShift;
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param state="value"> value </param>
        public void PackUInt8(byte value)
        {
            Bunch |= new BigInteger(value) << ActualPackingShift;
            ActualPackingShift += (int)DataSize.UInt8;
            Debug.Assert(ActualPackingShift <= (int)DataSize.MaxStorageSize);
#if ENABLE_U256STORAGE_VALIDATION
            DataList.Add(DataSize.UInt8);
#endif
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param state="value"> value </param>
        public void PackUInt16(UInt16 value)
        {
            Bunch |= new BigInteger(value) << ActualPackingShift;
            ActualPackingShift += (int)DataSize.UInt16;
            Debug.Assert(ActualPackingShift <= (int)DataSize.MaxStorageSize);
#if ENABLE_U256STORAGE_VALIDATION
            DataList.Add(DataSize.UInt16);
#endif
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param state="value"> value </param>
        public void PackUInt32(UInt32 value)
        {
            Bunch |= new BigInteger(value) << ActualPackingShift;
            ActualPackingShift += (int)DataSize.UInt32;
            Debug.Assert(ActualPackingShift <= (int)DataSize.MaxStorageSize);
#if ENABLE_U256STORAGE_VALIDATION
            DataList.Add(DataSize.UInt32);
#endif
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param state="value"> value </param>
        public void PackUInt64(UInt64 value)
        {
            Bunch |= new BigInteger(value) << ActualPackingShift;
            ActualPackingShift += (int)DataSize.UInt64;
            Debug.Assert(ActualPackingShift <= (int)DataSize.MaxStorageSize);
#if ENABLE_U256STORAGE_VALIDATION
            DataList.Add(DataSize.UInt64);
#endif
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param state="value"> value </param>
        public void PackBool(bool value)
        {
            PackUInt8((byte)(value == true ? 0xff : 0x00));
        }


        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public byte UnpackUInt8()
        {
#if ENABLE_U256STORAGE_VALIDATION
            Debug.Assert(DataList.Count > 0);
            Debug.Assert(DataList[0] == DataSize.UInt8);
            DataList.RemoveAt(0);
#endif
            byte value = (byte)((Bunch >> ActualUnpackingShift) & 0xff);
            ActualUnpackingShift += (int)DataSize.UInt8;
            Debug.Assert(ActualUnpackingShift <= (int)DataSize.MaxStorageSize);
            return value;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public UInt16 UnpackUInt16()
        {
#if ENABLE_U256STORAGE_VALIDATION
            Debug.Assert(DataList.Count > 0);
            Debug.Assert(DataList[0] == DataSize.UInt16);
            DataList.RemoveAt(0);
#endif
            UInt16 value = (UInt16)((Bunch >> ActualUnpackingShift) & 0xffff);
            ActualUnpackingShift += (int)DataSize.UInt16;
            Debug.Assert(ActualUnpackingShift <= (int)DataSize.MaxStorageSize);
            return value;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public UInt32 UnpackUInt32()
        {
#if ENABLE_U256STORAGE_VALIDATION
            Debug.Assert(DataList.Count > 0);
            Debug.Assert(DataList[0] == DataSize.UInt32);
            DataList.RemoveAt(0);
#endif
            UInt32 value = (UInt32)((Bunch >> ActualUnpackingShift) & 0xffffffff);
            ActualUnpackingShift += (int)DataSize.UInt32;
            Debug.Assert(ActualUnpackingShift <= (int)DataSize.MaxStorageSize);
            return value;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public UInt64 UnpackUInt64()
        {
#if ENABLE_U256STORAGE_VALIDATION
            Debug.Assert(DataList.Count > 0);
            Debug.Assert(DataList[0] == DataSize.UInt64);
            DataList.RemoveAt(0);
#endif
            UInt64 value = (UInt64)((Bunch >> ActualUnpackingShift) & 0xffffffffffffffff);
            ActualUnpackingShift += (int)DataSize.UInt64;
            Debug.Assert(ActualUnpackingShift <= (int)DataSize.MaxStorageSize);
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
